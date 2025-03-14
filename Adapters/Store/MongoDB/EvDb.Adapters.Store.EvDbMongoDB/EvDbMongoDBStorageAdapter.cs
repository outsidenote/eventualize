// Ignore Spelling: Mongo

using EvDb.Adapters.Store.EvDbMongoDB.Internals;
using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

    #region Ctor

    public EvDbMongoDBStorageAdapter(
                        MongoClientSettings settings,
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        var client = new MongoClient(settings);
        _collectionsSetup = CollectionsSetup.Create(logger, client, storageContext);    

        _logger = logger;
        _transformers = transformers.ToImmutableArray();
    }

    #endregion //  Ctor

    #region GetEventsAsync

    /// <summary>
    /// Retrieves events for the given stream cursor.
    /// </summary>
    public async IAsyncEnumerable<EvDbEvent> GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var eventsCollection = await _collectionsSetup.EventsCollectionTask;

        var filter = streamCursor.ToFilter();
        IFindFluent<BsonDocument, BsonDocument> query = eventsCollection.Find(filter)
                                     .Sort(QueryProvider.SortEvents);
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogQuery(query.ToJson());

        // TODO: [bnaya 2025-03-05] validate it gets all the data
        using IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync(cancellation);

        while (await cursor.MoveNextAsync(cancellation))
        {
            foreach (var doc in cursor.Current)
            {
                // Convert from BsonDocument back to EvDbEvent.
                yield return doc.ToEvent();
            }
        }
    }

    #endregion //  GetEventsAsync

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

        if (messages.Count == 0)
        {
            await StoreEventsAsync(options, eventDocs, null, cancellation);
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

            using var session = await db.Client.StartSessionAsync(cancellationToken: cancellation);
            session.StartTransaction(); // TODO: use transaction scope
            try
            {
                await StoreEventsAsync(options, eventDocs, session, cancellation);
                if (messages.Count != 0)
                {
                    outboxCountPerShard = await StoreOutbox();
                }

                await session.CommitTransactionAsync(cancellation);
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

            #region StoreOutboxAsync

            async Task<ImmutableDictionary<EvDbShardName, int>> StoreOutbox()
            {
                EvDbStreamAddress address = messages[0].StreamCursor;

                var tasks = shards.Select(async g =>
                {
                    EvDbShardName shardName = g.Key;
                    OtelTags tags = OtelTags.Empty.Add("shard", shardName);
                    using Activity? activity = _trace.StartActivity(tags, "EvDb.StoreOutboxAsync");
                    var outboxDocs = g.Select(m => m.EvDbToBsonDocument(shardName)).ToArray();

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

        return new StreamStoreAffected(events.Count, outboxCountPerShard);

        #region StoreEventsAsync

        async Task StoreEventsAsync(
                            InsertManyOptions options,
                            IEnumerable<BsonDocument> eventDocs,
                            IClientSessionHandle? session,
                            CancellationToken cancellation)
        {
            using var activity = _trace.StartActivity("EvDb.StoreEventsAsync");
            if(session == null)
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
        FilterDefinition<BsonDocument> filter = viewAddress.ToFilter();
        if (_logger.IsEnabled(LogLevel.Trace))
            _logger.LogQuery(filter.ToJson());

        IMongoCollection<BsonDocument> snapshotsCollection = await _collectionsSetup.SnapshotsCollectionTask;
        var document = await snapshotsCollection.Find(filter)
                                    .Sort(QueryProvider.SortSnapshots)
                                    .Project(QueryProvider.ProjectionSnapshots)
                                    .FirstOrDefaultAsync(cancellation);
        if (document == null)
            return EvDbStoredSnapshot.Empty;

        var snapshot =  document.ToSnapshotInfo();
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

        var snapshotDoc = snapshotData.EvDbToBsonDocument();
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
