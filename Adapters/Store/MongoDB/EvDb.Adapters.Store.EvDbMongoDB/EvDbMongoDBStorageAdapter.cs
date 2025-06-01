// Ignore Spelling: Mongo

using EvDb.Adapters.Internals;
using EvDb.Adapters.Store.Internals;
using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.Core.Adapters.Internals;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames.Fields;
using static EvDb.Core.Adapters.StoreTelemetry;

namespace EvDb.Adapters.Store.MongoDB;


/// <summary>
/// MongoDB storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbMongoDBStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter, IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    private const string DATABASE_TYPE = "MongoDB";
    private const int DEFAUL_EMTRY_CHANGES_COMPANSATION_SECONDS = 3;
    private const int DEFAUL_BATCH_SITE_ON_HISTORY_TRANSITION = 100;
    private readonly CollectionsSetup _collectionsSetup;
    private static readonly TimeSpan CATCHUP_TIMEOUT = Debugger.IsAttached
                                        ? TimeSpan.FromSeconds(10)
                                        : TimeSpan.FromMinutes(10);

    #region Ctor

    #region Overloads

    public EvDbMongoDBStorageAdapter(
                        MongoClientSettings settings,
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers,
                        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
                            : this(new MongoClient(settings), logger, storageContext, transformers, creationMode)
    {
    }

    #endregion //  Overloads

    private EvDbMongoDBStorageAdapter(
                        MongoClient client,
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers,
                        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None,
                        TimeProvider? timeProvider = null)
    {
        _collectionsSetup = CollectionsSetup.Create(logger, client, storageContext, creationMode);

        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _transformers = transformers.ToImmutableArray();
    }

    #endregion //  Ctor

    #region GetEventsAsync

    async IAsyncEnumerable<EvDbEvent> IEvDbStorageStreamAdapter.GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation)
    {
        var eventsCollection = await _collectionsSetup.EventsCollectionTask;

        var options = EvDbContinuousFetchOptions.CompleteIfEmpty;
        var parameters = new EvDbGetEventsParameters(streamCursor);
        int attemptsWhenEmpty = 0;
        TimeSpan delay = options.DelayWhenEmpty.StartDuration;
        while (!cancellation.IsCancellationRequested)
        {
            FilterDefinition<BsonDocument> filter = parameters.ToBsonFilter();
            IFindFluent<BsonDocument, BsonDocument> query = eventsCollection.Find(filter)
                                         .Sort(QueryProvider.SortEvents)
                                         .Limit(parameters.BatchSize);
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogQuery(query.ToJson());

            using IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync(cancellation);

            EvDbEvent? last = null;
            int count = 0;
            while (!cancellation.IsCancellationRequested && 
                   await cursor.MoveNextAsync(cancellation))
            {
                foreach (var doc in cursor.Current)
                {
                    // Convert from BsonDocument back to EvDbEvent.
                    var @event = doc.ToEvent();
                    last = @event;
                    count++;
                    yield return @event;
                }
            }

            bool reachTheEnd = count < options.BatchSize;
            (delay, attemptsWhenEmpty, bool shouldExit) = await options.DelayWhenEmptyAsync(
                                                                  reachTheEnd,
                                                                  delay,
                                                                  attemptsWhenEmpty,
                                                                  cancellation);
            if (shouldExit)
                break;
            parameters = parameters.ContinueFrom(last);
        }
    }

    #endregion //  GetEventsAsync

    #region GetMessagesAsync

    async IAsyncEnumerable<EvDbMessage> IEvDbStorageStreamAdapter.GetMessagesAsync(
                            EvDbShardName shardName,
                            EvDbMessageFilter filter,
                            EvDbContinuousFetchOptions? options,
                            [EnumeratorCancellation] CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        var opts = options ?? EvDbContinuousFetchOptions.ContinueIfEmpty;
        var batchOptions = EvDbContinuousFetchOptions.CompleteIfEmpty;
        IMongoCollection<BsonDocument> collection =
                        await _collectionsSetup.CreateOutboxCollectionIfNotExistsAsync(shardName);


        var parameters = new EvDbGetMessagesParameters(filter, options ?? EvDbContinuousFetchOptions.ContinueIfEmpty);

        int attemptsWhenEmpty = 0;
        TimeSpan delay = opts.DelayWhenEmpty.StartDuration;
        EvDbMessage? last = null;

        #region Iterate over the history

        while (!cancellation.IsCancellationRequested)
        {
            FilterDefinition<BsonDocument> bsonFilter = parameters.ToBsonFilter();

            IFindFluent<BsonDocument, BsonDocument> query = collection.Find(bsonFilter)
                                 .Sort(QueryProvider.SortMessages)
                                 .Limit(parameters.BatchSize);

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogQuery(query.ToJson());

            using IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync(cancellation);

            bool hasRows = await cursor.MoveNextAsync(cancellation);
            while (hasRows && !cancellation.IsCancellationRequested)
            {
                foreach (var doc in cursor.Current)
                {
                    // Convert from BsonDocument back to EvDbEvent.
                    EvDbMessage message = doc.ToMessage();
                    last = message;
                    yield return message;
                }
                (delay, attemptsWhenEmpty, bool shouldExit) = await batchOptions.DelayWhenEmptyAsync(
                                                                      hasRows,
                                                                      delay,
                                                                      attemptsWhenEmpty,
                                                                      cancellation);
                if (shouldExit)
                    break;

                parameters = parameters.ContinueFrom(last);
            }
        }

        #endregion //  Iterate over the history

        if (opts.CompleteWhenEmpty)
            yield break;

        var pipeline = parameters.ToBsonPipeline();

        #region ChangeStreamOptions watchOptions = ...

        var statTime = last?.StoredAt ?? DateTimeOffset.UtcNow.AddSeconds(-DEFAUL_EMTRY_CHANGES_COMPANSATION_SECONDS);
        var startAt = new BsonTimestamp((int)statTime.ToUnixTimeSeconds(), 0);
        ChangeStreamOptions watchOptions = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.WhenAvailable,
            MaxAwaitTime = TimeSpan.FromSeconds(1), // means use server default (infinite wait)
            StartAtOperationTime = startAt, // Start at the latest operation time
                                            //StartAfter = ,
            BatchSize = null // The server will choose a batch size dynamically based on internal optimizations
        };

        #endregion // ChangeStreamOptions watchOptions = ...

        var duplicationLookup = new ConcurrentDictionary<EvDbMessageId, object?>();
        Task<(EvDbMessage? FirstMessage, BsonDocument? ResumeToken)> firstChangedMessageTask =
                            WaitForFirstMessageAsync(collection, pipeline, watchOptions, cancellation);

        #region Catch up racing with the change stream

        using var timeout = new CancellationTokenSource(CATCHUP_TIMEOUT, _timeProvider);
        using (var catchupCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation, timeout.Token))
        {
            CancellationToken catchupCancellation = catchupCancellationSource.Token;
            do
            {
                FilterDefinition<BsonDocument> bsonCatchUpFilter = parameters.ToBsonFilter();
                IFindFluent<BsonDocument, BsonDocument> queryCatchUp = collection.Find(bsonCatchUpFilter)
                             .Sort(QueryProvider.SortMessages)
                             .Limit(DEFAUL_BATCH_SITE_ON_HISTORY_TRANSITION);

                if (_logger.IsEnabled(LogLevel.Trace))
                    _logger.LogQuery(queryCatchUp.ToJson());

                using IAsyncCursor<BsonDocument> cursorCatchUp = await queryCatchUp.ToCursorAsync(catchupCancellation);

                bool hasRowsCatchUp = await cursorCatchUp.MoveNextAsync(cancellation);
                while (hasRowsCatchUp && !catchupCancellation.IsCancellationRequested)
                {
                    foreach (var doc in cursorCatchUp.Current)
                    {
                        // Convert from BsonDocument back to EvDbEvent.
                        EvDbMessage message = doc.ToMessage();
                        duplicationLookup.TryAdd(message.Id, null);

                        yield return message;
                    }
                }
            } while (!catchupCancellation.IsCancellationRequested && !firstChangedMessageTask.IsCompleted);
        }

        #endregion //  Catch up racing with the change stream

        if (cancellation.IsCancellationRequested)
            yield break;

        (EvDbMessage? firstMessage, BsonDocument? resumeToken) = await firstChangedMessageTask;

        if (firstMessage != null)
            yield return firstMessage.Value;

        await foreach (var m in WatchWithDuplicationCheckAsync())
            yield return m;

        await foreach (var m in WatchWithoutDuplicationCheckAsync())
            yield return m;

        #region WatchWithDuplicationCheckAsync

        async IAsyncEnumerable<EvDbMessage> WatchWithDuplicationCheckAsync()
        {
            #region watchOptions = ...

            if (resumeToken != null)
            {
                watchOptions.ResumeAfter = resumeToken; // Use the resume token from the first message
                watchOptions.StartAtOperationTime = null; // Clear StartAtOperationTime to use the resume token
            }
            watchOptions.MaxAwaitTime = null; // Use the server default (infinite wait)

            #endregion //  MyRegion

            while (!cancellation.IsCancellationRequested && duplicationLookup.Count != 0)
            {
                using var changes = await collection.WatchAsync(pipeline, watchOptions, cancellation);
                if (await changes.MoveNextAsync(cancellation))
                {
                    foreach (var change in changes.Current)
                    {
                        resumeToken = change.ResumeToken;
                        var message = change.FullDocument.ToMessage();
                        if (duplicationLookup.TryRemove(message.Id, out _))
                        {
                            // If the message was handled.
                            continue;
                        }
                        yield return message;
                    }
                }
            }
        }

        #endregion //  WatchWithDuplicationCheckAsync

        #region WatchWithoutDuplicationCheckAsync

        async IAsyncEnumerable<EvDbMessage> WatchWithoutDuplicationCheckAsync()
        {
            #region watchOptions = ...

            if (resumeToken != null)
            {
                watchOptions.ResumeAfter = resumeToken; // Use the resume token from the first message
                watchOptions.StartAtOperationTime = null; // Clear StartAtOperationTime to use the resume token
            }
            watchOptions.MaxAwaitTime = null; // Use the server default (infinite wait)

            #endregion //  MyRegion

            while (!cancellation.IsCancellationRequested)
            {
                using var changes = await collection.WatchAsync(pipeline, watchOptions, cancellation);
                if (await changes.MoveNextAsync(cancellation))
                {
                    foreach (var change in changes.Current)
                    {
                        var message = change.FullDocument.ToMessage();
                        yield return message;
                    }
                }
            }
        }

        #endregion //  WatchWithoutDuplicationCheckAsync

        #region WaitForFirstMessageAsync

        static async Task<(EvDbMessage? FirstMessage, BsonDocument? ResumeToken)> WaitForFirstMessageAsync(
                                                                 IMongoCollection<BsonDocument> collection,
                                                                 PipelineDefinition<ChangeStreamDocument<BsonDocument>, ChangeStreamDocument<BsonDocument>> pipeline,
                                                                 ChangeStreamOptions watchOptions,
                                                                 CancellationToken cancellation)
        {
            BsonDocument? resumeToken = null;
            while (!cancellation.IsCancellationRequested)
            {
                using var changes = await collection.WatchAsync(pipeline, watchOptions, cancellation);
                if (await changes.MoveNextAsync(cancellation))
                {
                    var change = changes.Current.FirstOrDefault();
                    var firstChangedMessage = change?.FullDocument.ToMessage();
                    resumeToken = change?.ResumeToken;
                    return (firstChangedMessage, resumeToken);
                }
            }

            return (null, null);
        }

        #endregion //  WaitForFirstMessageAsync
    }

    #endregion //  GetMessagesAsync

    #region GetLastEventAsync

    async Task<long> IEvDbStorageStreamAdapter.GetLastOffsetAsync(
    EvDbStreamAddress address,
    CancellationToken cancellation)
    {
        var eventsCollection = await _collectionsSetup.EventsCollectionTask;

        var filter = address.ToBsonFilter();
        IFindFluent<BsonDocument, BsonDocument> query = eventsCollection.Find(filter)
                                     .Sort(QueryProvider.SortEventsDesc)
                                     .Project(QueryProvider.ProjectionOffset)
                                     .Limit(1);
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogQuery(query.ToJson());

        BsonDocument doc = await query.FirstOrDefaultAsync(cancellation);
        long result = doc?[Fields.Event.Offset].AsInt64 ?? 0;
        return result;
    }

    #endregion //  GetLastOffsetAsync

    #region StoreStreamAsync

    async Task<Core.StreamStoreAffected> IEvDbStorageStreamAdapter.StoreStreamAsync(IImmutableList<EvDbEvent> events,
                                                                              IImmutableList<EvDbMessage> messages,
                                                                              CancellationToken cancellation)
    {
        #region Validation

        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (messages == null)
            throw new ArgumentNullException(nameof(messages));
        if (events.Count == 0)
            return new StreamStoreAffected(0, ImmutableDictionary<EvDbShardName, int>.Empty);

        #endregion //  Validation

        var options = new InsertManyOptions { IsOrdered = true };

        // Convert events and messages to BsonDocument lists.
        var eventDocs = events.Select(e => e.EvDbToBsonDocument());

        IImmutableDictionary<EvDbShardName, int> outboxCountPerShard = ImmutableDictionary<EvDbShardName, int>.Empty;
        IMongoDatabase db = _collectionsSetup.Db;
        var eventsCollection = await _collectionsSetup.EventsCollectionTask;

        using var session = await db.Client.StartSessionAsync(cancellationToken: cancellation);
        bool inInExternalTransaction = session.IsInTransaction;

        if (!inInExternalTransaction)
            session.StartTransaction();
        try
        {
            if (messages.Count == 0)
            {
                try
                {
                    await StoreEventsAsync(options, eventDocs, null, cancellation);
                }
                #region Exception Handling

                catch (MongoBulkWriteException<BsonDocument> ex)
                {
                    ServerErrorCategory? cateory = ex.WriteErrors.FirstOrDefault()?.Category;
                    if (cateory == ServerErrorCategory.DuplicateKey)
                    {
                        var address = events[0].StreamCursor;
                        var cursor = new EvDbStreamCursor(address);
                        throw new OCCException(cursor, ex);
                    }
                    throw;
                }
                catch (MongoCommandException ex) // when ex.Message.StartsWith("Command insert failed: Caused by ::  :: Please retry your operation or multi-document transaction..")
                {
                    var address = events[0].StreamCursor;
                    var cursor = new EvDbStreamCursor(address);
                    throw new OCCException(cursor, ex);
                }

                #endregion //  Exception Handling
            }
            else
            {
                IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> shards =
                                                    messages.GroupByShards(_transformers);
                // make sure the collection exists before starting the transaction
                var tasks = shards.Select(async shard =>
                {
                    await _collectionsSetup.CreateOutboxCollectionIfNotExistsAsync(shard.Key);
                });
                await Task.WhenAll(tasks);

                await StoreEventsAsync(options, eventDocs, session, cancellation);
                if (messages.Count != 0)
                {
                    outboxCountPerShard = await StoreOutbox();
                }

                if (!inInExternalTransaction)
                    await session.CommitTransactionAsync(cancellation);

                #region StoreOutboxAsync

                async Task<ImmutableDictionary<EvDbShardName, int>> StoreOutbox()
                {
                    EvDbStreamAddress address = messages[0].StreamCursor;

                    var tasks = shards.Select(async g =>
                    {
                        EvDbShardName shardName = g.Key;
                        OtelTags tags = OtelTags.Empty.Add("shard", shardName);
                        using Activity? activity = _trace.StartActivity(tags, "EvDb.StoreOutboxAsync");
                        var outboxDocs = g.Select(m => m.EvDbToBsonDocument(shardName))
                                          .ToArray();

                        var outboxCollection = await _collectionsSetup.CreateOutboxCollectionIfNotExistsAsync(shardName);
                        await outboxCollection.InsertManyAsync(session, outboxDocs, options, cancellation);
                        int affctedMessages = outboxDocs.Length;
                        StoreMeters.AddMessages(affctedMessages, address, DATABASE_TYPE, shardName);
                        return KeyValuePair.Create(shardName, affctedMessages);
                    });
                    var pairs = await Task.WhenAll(tasks);
                    var result = pairs.ToImmutableDictionary();
                    return result;
                }

                #endregion //  StoreOutboxAsync
            }
        }
        #region Exception Handling

        catch (MongoBulkWriteException<BsonDocument> ex)
        {
            var cursor = events[0].StreamCursor;
            await session.AbortTransactionAsync(cancellation);
            throw new OCCException(cursor, ex);
        }
        catch (MongoCommandException ex) // when ex.Message.StartsWith("Command insert failed: Caused by ::  :: Please retry your operation or multi-document transaction..")
        {
            await session.AbortTransactionAsync(cancellation);
            var address = events[0].StreamCursor;
            var cursor = new EvDbStreamCursor(address);
            throw new OCCException(cursor, ex);
        }
        catch (Exception)
        {
            await session.AbortTransactionAsync(cancellation);
            throw;
        }

        #endregion //  Exception Handling

        return new StreamStoreAffected(events.Count, outboxCountPerShard);

        #region StoreEventsAsync

        async Task StoreEventsAsync(
                            InsertManyOptions options,
                            IEnumerable<BsonDocument> eventDocs,
                            IClientSessionHandle? session,
                            CancellationToken cancellation)
        {
            using var activity = _trace.StartActivity("EvDb.StoreEventsAsync");
            if (session == null)
                await eventsCollection.InsertManyAsync(eventDocs, options, cancellation);
            else
                await eventsCollection.InsertManyAsync(session, eventDocs, options, cancellation);
            int affctedEvents = events.Count;
            EvDbStreamAddress address = events[0].StreamCursor;
            StoreMeters.AddEvents(affctedEvents, address, DATABASE_TYPE);
        }

        #endregion //  StoreEventsAsync
    }

    #endregion //  StoreStreamAsync

    #region GetSnapshotAsync

    /// <summary>
    /// Retrieves a stored snapshot for the specified view address.
    /// </summary>
    async Task<EvDbStoredSnapshot> IEvDbStorageSnapshotAdapter.GetSnapshotAsync(
                                                EvDbViewAddress viewAddress,
                                                CancellationToken cancellation)
    {
        FilterDefinition<BsonDocument> filter = viewAddress.ToBsonFilter();
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogQuery(filter.ToJson());

        IMongoCollection<BsonDocument> snapshotsCollection = await _collectionsSetup.SnapshotsCollectionTask;
        var document = await snapshotsCollection.Find(filter)
                                    .Sort(QueryProvider.SortSnapshots)
                                    .Project(QueryProvider.ProjectionSnapshots)
                                    .FirstOrDefaultAsync(cancellation);
        if (document == null)
            return EvDbStoredSnapshot.Empty;

        var snapshot = document.ToSnapshotInfo();
        return snapshot;
    }

    #endregion //  GetSnapshotAsync

    #region StoreSnapshotAsync

    /// <summary>
    /// Stores a snapshot.
    /// </summary>
    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
    {
        #region Validation

        if (snapshotData == null)
            throw new ArgumentNullException(nameof(snapshotData));

        #endregion //  Validation

        BsonDocument snapshotDoc = snapshotData.EvDbToBsonDocument();
        try
        {
            IMongoCollection<BsonDocument> snapshotsCollection = await _collectionsSetup.SnapshotsCollectionTask;

            await snapshotsCollection.InsertOneAsync(snapshotDoc, cancellationToken: cancellation);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException("Optimistic concurrency error while inserting snapshot.", ex);
        }
    }

    #endregion //  StoreSnapshotAsync

    #region Dispose Pattern

    void IDisposable.Dispose()
    {
        DisposeAction();
        GC.SuppressFinalize(this);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        IAsyncDisposable setup = _collectionsSetup;
        await setup.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private void DisposeAction()
    {
        IDisposable setup = _collectionsSetup;
        setup.Dispose();
    }

    #endregion //  DisposeAction Pattern
}
