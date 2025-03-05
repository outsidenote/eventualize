// Ignore Spelling: Mongo

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using EvDb.Adapters.Store.EvDbMongoDB.Internals;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.Latency;
using System.Collections.Concurrent;
using EvDb.Adapters.Store.Internals;
using static EvDb.Core.Adapters.StoreTelemetry;

namespace EvDb.Adapters.Store.MongoDB;

// TODO: [bnaya 2025-02-23] Add OTEL, Logs
// TODO: [bnaya 2025-02-23] replace string with nameof

public record EvDbCollections(IMongoCollection<BsonDocument> Events,
                              IMongoCollection<BsonDocument> Snapshots);

/// <summary>
/// MongoDB storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbMongoDBStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter, IDisposable, IAsyncDisposable
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<BsonDocument> _eventsCollection;
    private readonly IMongoCollection<BsonDocument> _snapshotsCollection;
    private readonly ConcurrentDictionary<EvDbShardName, IMongoCollection<BsonDocument>> _outboxCollections = new ConcurrentDictionary<EvDbShardName, IMongoCollection<BsonDocument>>();
    private readonly string _outboxCollectionFormat;
    private readonly ILogger _logger;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private readonly static ActivitySource _trace = StoreTelemetry.Trace;
    private const string DATABASE_TYPE = "MongoDB";


    #region Ctor

    public EvDbMongoDBStorageAdapter(
                        MongoClientSettings settings,
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        string collectionPrefix = storageContext.CalcCollectionPrefix();

        _client = new MongoClient(settings);
        string databaseName = storageContext.DatabaseName;
        _db = _client.GetDatabase(databaseName);


        _eventsCollection = _db.GetCollection<BsonDocument>($"{collectionPrefix}events");
        _snapshotsCollection = _db.GetCollection<BsonDocument>($"{collectionPrefix}snapshots");
        _outboxCollectionFormat = $$"""{{collectionPrefix}}{0}outbox""";

        // TODO: [bnaya 2025-02-23] Migration? outside of Ctor, index are important
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
        var filter = streamCursor.ToFilter();
        var sorting = Builders<BsonDocument>.Sort.Ascending("_id");

        IFindFluent<BsonDocument, BsonDocument> query = _eventsCollection.Find(filter)
                                     .Sort(sorting);
        if(_logger.IsEnabled(LogLevel.Trace))
            _logger.LogQuery(query.ToJson());

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
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (messages == null)
            throw new ArgumentNullException(nameof(messages));
        if (events.Count == 0)
            return new StreamStoreAffected(0, ImmutableDictionary<EvDbShardName, int>.Empty);

        var options = new InsertManyOptions { IsOrdered = true };

        // Convert events and messages to BsonDocument lists.
        var eventDocs = events.Select(e => e.EvDbToBsonDocument());

        IImmutableDictionary<EvDbShardName, int> outboxCountPerShard = ImmutableDictionary<EvDbShardName, int>.Empty;
        using var session = await _db.Client.StartSessionAsync(cancellationToken: cancellation);
        session.StartTransaction();
        try
        {
            await StoreEventsAsync(options, eventDocs, session, cancellation);
            if (messages.Count != 0)
            {
                outboxCountPerShard = await StoreOutbox(session);
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
        catch (Exception)
        {
            await session.AbortTransactionAsync(cancellation);
            throw;
        }

        #endregion //  Exception Handling

        return new StreamStoreAffected(events.Count, outboxCountPerShard);

        #region StoreOutboxAsync

        async Task<ImmutableDictionary<EvDbShardName, int>> StoreOutbox(IClientSessionHandle session)
        {
            EvDbStreamAddress address = messages[0].StreamCursor;
            IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> shards =
                                    messages.GroupByShards(_transformers);
            var tasks = shards.Select(async g =>
            {
                EvDbShardName shardName = g.Key;
                OtelTags tags = OtelTags.Empty.Add("shard", shardName);
                using Activity? activity = _trace.StartActivity(tags, "EvDb.StoreOutboxAsync");
                var outboxDocs = g.Select(m => m.EvDbToBsonDocument(shardName)).ToArray();
                var outboxCollection = _outboxCollections.GetOrAdd(shardName, CreateOutboxCollection);
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

        #region StoreEventsAsync

        async Task StoreEventsAsync(
                            InsertManyOptions options,
                            IEnumerable<BsonDocument> eventDocs,
                            IClientSessionHandle session,
                            CancellationToken cancellation)
        {
            using var activity = _trace.StartActivity("EvDb.StoreEventsAsync");
            await _eventsCollection.InsertManyAsync(session, eventDocs, options, cancellation);
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

        var document = await _snapshotsCollection.Find(filter).FirstOrDefaultAsync(cancellation);
        if (document == null)
            return EvDbStoredSnapshot.Empty;

        return document.ToSnapshotInfo();
    }

    #endregion //  GetSnapshotAsync

    #region StoreSnapshotAsync

    /// <summary>
    /// Stores a snapshot.
    /// </summary>
    async Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
    {
        if (snapshotData == null)
            throw new ArgumentNullException(nameof(snapshotData));

        var snapshotDoc = snapshotData.EvDbToBsonDocument();
        try
        {
            await _snapshotsCollection.InsertOneAsync(snapshotDoc, cancellationToken: cancellation);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new InvalidOperationException("Optimistic concurrency error while inserting snapshot.", ex);
        }
    }

    #endregion //  StoreSnapshotAsync

    #region CreateOutboxCollection

    private IMongoCollection<BsonDocument> CreateOutboxCollection(EvDbShardName shard)
    {
        string collectionName = string.Format(_outboxCollectionFormat, shard);
        return _db.GetCollection<BsonDocument>(collectionName);
    }

    #endregion //  CreateOutboxCollection

    #region Dispose Pattern

    void IDisposable.Dispose()
    {
        DisposeAction();
        GC.SuppressFinalize(this);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        DisposeAction();
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private void DisposeAction()
    {
        _client?.Dispose();
    }

    ~EvDbMongoDBStorageAdapter()
    {
        DisposeAction();
    }

    #endregion //  DisposeAction Pattern
}
