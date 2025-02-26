// Ignore Spelling: Mongo

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using EvDb.Adapters.Store.EvDbMongoDB.Internals;

namespace EvDb.Adapters.Store.MongoDB;

// TODO: [bnaya 2025-02-23] Add OTEL, Logs
// TODO: [bnaya 2025-02-23] replace string with nameof

/// <summary>
/// MongoDB storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
internal sealed class EvDbMongoDBStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter, IDisposable, IAsyncDisposable
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<BsonDocument> _eventsCollection;
    private readonly IMongoCollection<BsonDocument> _snapshotsCollection;
    private readonly IMongoCollection<BsonDocument> _outboxCollection;
    private readonly ILogger _logger;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;


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

        // Collections for events, snapshots, and outbox messages.
        _eventsCollection = _db.GetCollection<BsonDocument>($"{collectionPrefix}events");
        _snapshotsCollection = _db.GetCollection<BsonDocument>($"{collectionPrefix}snapshots");
        // TODO: [Bnaya 2025-02-20] dynamic table 
        _outboxCollection = _db.GetCollection<BsonDocument>($$"""{{collectionPrefix}}{0}outbox""");

        // TODO: [bnaya 2025-02-23] Migration? outside of Ctor
        _logger = logger;
        _transformers = transformers.ToImmutableArray();
    }

    #region GetEventsAsync

    /// <summary>
    /// Retrieves events for the given stream cursor.
    /// </summary>
    public async IAsyncEnumerable<EvDbEvent> GetEventsAsync(
        EvDbStreamCursor streamCursor,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        // For demonstration purposes, assume streamCursor has properties StreamId and Offset.
        var filter = Builders<BsonDocument>.Filter.Eq("stream_id", streamCursor.StreamId) &
                     Builders<BsonDocument>.Filter.Gt("offset", streamCursor.Offset);

        using (var cursor = await _eventsCollection.FindAsync(filter, cancellationToken: cancellation))
        {
            while (await cursor.MoveNextAsync(cancellation))
            {
                foreach (var doc in cursor.Current)
                {
                    // Convert from BsonDocument back to EvDbEvent.
                    yield return doc.ToEvent();
                }
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
        var eventDocs = events.Select(e => e.ToBsonDocument()).ToList();

        using (var session = await _db.Client.StartSessionAsync(cancellationToken: cancellation))
        {
            session.StartTransaction();
            try
            {
                await _eventsCollection.InsertManyAsync(session, eventDocs, options, cancellation);
                if (messages.Count != 0)
                {
                    await StoreOutbox(session);
                }

                await session.CommitTransactionAsync(cancellation);
            }
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
        }

        // TODO: [Bnaya 2025-02-19] make it more efficient
        ImmutableDictionary<EvDbShardName, int> outboxMap =
            messages.GroupBy(m => m.ShardName)
                                .Select(g => new { g.Key, Count = g.Count() })
                                .ToDictionary(m => m.Key, m => m.Count)
                                .ToImmutableDictionary();
        return new StreamStoreAffected(events.Count, outboxMap);

        async Task StoreOutbox(IClientSessionHandle session)
        {
            EvDbStreamAddress address = messages[0].StreamCursor;
            IEnumerable<IGrouping<EvDbShardName, EvDbMessageRecord>> shards =
                                    messages.GroupByShards(_transformers);

            var outboxDocs = messages.Select(m => m.ToBsonDocument()).ToList();
            await _outboxCollection.InsertManyAsync(session, outboxDocs, options, cancellation);
        }
    }

    #endregion //  StoreStreamAsync

    #region GetSnapshotAsync

    /// <summary>
    /// Retrieves a stored snapshot for the specified view address.
    /// </summary>
    public async Task<EvDbStoredSnapshot> GetSnapshotAsync(EvDbViewAddress viewAddress, CancellationToken cancellation = default)
    {
        // For demonstration, assume viewAddress has properties Domain, Partition, and StreamId.
        var filter = Builders<BsonDocument>.Filter.Eq("domain", viewAddress.Domain) &
                     Builders<BsonDocument>.Filter.Eq("partition", viewAddress.Partition) &
                     Builders<BsonDocument>.Filter.Eq("stream_id", viewAddress.StreamId);

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

        var snapshotDoc = snapshotData.ToBsonDocument();
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
