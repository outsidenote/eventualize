// Ignore Spelling: Mongo

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace EvDb.Adapters.Store.MongoDB;

// TODO: [bnaya 2025-02-23] Add OTEL, Logs
// TODO: [bnaya 2025-02-23] replace string with nameof

/// <summary>
/// MongoDB storage adapter that handles event streams, snapshots, and outbox messages.
/// </summary>
public class EvDbMongoDBStorageAdapter : IEvDbStorageStreamAdapter, IEvDbStorageSnapshotAdapter
{
    private readonly IMongoDatabase _db;
    private readonly IMongoCollection<BsonDocument> _eventsCollection;
    private readonly IMongoCollection<BsonDocument> _snapshotsCollection;
    private readonly IMongoCollection<BsonDocument> _outboxCollection;
    private readonly ILogger _logger;
    private readonly IImmutableList<IEvDbOutboxTransformer> _transformers;
    private readonly Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;


    public EvDbMongoDBStorageAdapter(
                        MongoClientSettings settings,
                        ILogger logger,
                        EvDbStorageContext storageContext,
                        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";

        var client = new MongoClient(settings);
        string databaseName = storageContext.DatabaseName;
        _db = client.GetDatabase(databaseName);

        // Collections for events, snapshots, and outbox messages.
        _eventsCollection = _db.GetCollection<BsonDocument>($"{tblInitial}events");
        _snapshotsCollection = _db.GetCollection<BsonDocument>($"{tblInitial}snapshots");
        // TODO: [Bnaya 2025-02-20] dynamic table 
        _outboxCollection = _db.GetCollection<BsonDocument>($$"""{{tblInitial}}{0}outbox""");

        // TODO: [bnaya 2025-02-23] Migration? outside of Ctor
        CreateUniqueIndex();
        _logger = logger;
        _transformers = transformers.ToImmutableArray();
    }


    // TODO: [bnaya 2025-02-23] create indices for outbox and snapshots
    // TODO: [bnaya 2025-02-23] generate a DDL script 
    private async Task CreateUniqueIndex()
    {
        var unique = new CreateIndexOptions { Unique = true };

        var eventsPK = Builders<BsonDocument>.IndexKeys
            .Ascending(toSnakeCase(nameof(EvDbEventRecord.Domain)))
            .Ascending(toSnakeCase(nameof(EvDbEventRecord.Partition)))
            .Ascending(toSnakeCase(nameof(EvDbEventRecord.StreamId)))
            .Ascending(toSnakeCase(nameof(EvDbEventRecord.Offset)));
        var eventsIndexModel = new CreateIndexModel<BsonDocument>(eventsPK, unique);
        await _eventsCollection.Indexes.CreateOneAsync(eventsIndexModel);

        var outboxPK = Builders<BsonDocument>.IndexKeys
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Domain)))
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Partition)))
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.StreamId)))
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Offset)))
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Channel)))
            .Ascending(toSnakeCase(nameof(EvDbMessageRecord.MessageType)));
        var outboxIndexModel = new CreateIndexModel<BsonDocument>(outboxPK, unique);
        await _outboxCollection.Indexes.CreateOneAsync(outboxIndexModel);

        var snapshotPK = Builders<BsonDocument>.IndexKeys
            .Ascending(toSnakeCase(nameof(EvDbViewAddress.Domain)))
            .Ascending(toSnakeCase(nameof(EvDbViewAddress.Partition)))
            .Ascending(toSnakeCase(nameof(EvDbViewAddress.StreamId)))
            .Ascending(toSnakeCase(nameof(EvDbViewAddress.ViewName)))
            .Ascending(toSnakeCase(nameof(EvDbStoredSnapshot.Offset)));
        var snapshotIndexModel = new CreateIndexModel<BsonDocument>(snapshotPK, unique);
        await _outboxCollection.Indexes.CreateOneAsync(snapshotIndexModel);
    }

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
}
