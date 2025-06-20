﻿// Ignore Spelling: Mongo

using EvDb.Adapters.Internals;
using EvDb.Adapters.Store.Internals;
using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.Core.Adapters.Internals;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;
using static EvDb.Core.Adapters.StoreTelemetry;

namespace EvDb.Adapters.Store.MongoDB;


/// <summary>
/// MongoDB storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbMongoDBStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter, IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    private const string DATABASE_TYPE = "MongoDB";
    private readonly CollectionsSetup _collectionsSetup;
    private bool _disposed;

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
                        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
    {
        _collectionsSetup = CollectionsSetup.Create(logger, client, storageContext, creationMode);

        _logger = logger;
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
            // Learn More on MongoDB query: https://claude.ai/public/artifacts/daa32caa-7884-44c3-bf6e-c99724c484af
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

            bool reachTheEnd = count < parameters.BatchSize;
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

    async IAsyncEnumerable<EvDbMessage> IEvDbChangeStream.GetMessagesAsync(
                            EvDbShardName shard,
                            EvDbMessageFilter filter,
                            EvDbContinuousFetchOptions? options,
                            [EnumeratorCancellation] CancellationToken cancellation)
    {
        if (cancellation.IsCancellationRequested)
            yield break;

        var opts = options ?? EvDbContinuousFetchOptions.ContinueIfEmpty;
        IMongoCollection<BsonDocument> collection =
                        await _collectionsSetup.CreateOutboxCollectionIfNotExistsAsync(shard);

        var parameters = new EvDbGetMessagesParameters(filter, options ?? EvDbContinuousFetchOptions.ContinueIfEmpty);

        int attemptsWhenEmpty = 0;
        TimeSpan delay = opts.DelayWhenEmpty.StartDuration;
        ObjectId? lastId = null;
        var duplicateDetection = new HashSet<Guid>();
        EvDbMessage? last = null;
        while (!cancellation.IsCancellationRequested)
        {
            IAsyncCursor<BsonDocument> cursor = await GetCursorAsync();

            while (await cursor.MoveNextAsync(cancellation).FalseWhenCancelAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    if (cancellation.IsCancellationRequested)
                        yield break; // Exit if cancellation is requested

                    lastId = doc.GetObjectId();
                    // Convert from BsonDocument back to EvDbEvent.
                    EvDbMessage message = doc.ToMessage();
                    if (duplicateDetection.Contains(message.Id))
                        continue; // Skip duplicate messages
                    ManageDuplicationList();
                    last = message;

                    yield return message;

                    #region ManageDuplicationList

                    void ManageDuplicationList()
                    {
                        if (last != null && last.Value.StoredAt != message.StoredAt)
                            duplicateDetection.Clear();
                        duplicateDetection.Add(message.Id);
                    }

                    #endregion //  ManageDuplicationList
                }
            }

            (delay, attemptsWhenEmpty, bool shouldExit) = await opts.DelayWhenEmptyAsync(
                                                                  true,
                                                                  delay,
                                                                  attemptsWhenEmpty,
                                                                  cancellation);
            if (shouldExit)
                yield break;
        }

        #region GetCursorAsync

        async Task<IAsyncCursor<BsonDocument>> GetCursorAsync()
        {
            var filter = parameters.ToBsonFilter(lastId);

            var query = collection
                .Find(filter)
                .Sort(QueryProvider.SortMessages);

            IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync(cancellation);

            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogQuery(query.ToJson());

            return cursor;
        }

        #endregion //  GetCursorAsync
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
        if(_disposed)
            return;
        _disposed = true;

        IDisposable setup = _collectionsSetup;
        setup.Dispose();
    }

    #endregion //  DisposeAction Pattern
}
