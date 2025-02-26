// Ignore Spelling: Mongo
// Ignore Spelling: Admin

using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using EvDb.Adapters.Store.EvDbMongoDB.Internals;
using EvDb.Core.Adapters;
using MongoDB.Bson;

namespace EvDb.Adapters.Store.Postgres;

public sealed class MongoStorageAdmin : IEvDbStorageAdmin, IDisposable, IAsyncDisposable
{
    private readonly MongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;
    private readonly ILogger _logger;
    private readonly Task<MongoCollections> _collections;

    private readonly record struct MongoCollections(
                            IMongoCollection<BsonDocument> EventsCollection,
                            IMongoCollection<BsonDocument> OutboxCollection,
                            IMongoCollection<BsonDocument> SnapshotsCollection);


    public MongoStorageAdmin(
        ILogger logger,
        MongoClientSettings settings,
        EvDbStorageContext storageContext, 
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        string collectionPrefix = storageContext.CalcCollectionPrefix();
        _client = new MongoClient(settings);
        string databaseName = storageContext.DatabaseName;
        _db = _client.GetDatabase(databaseName);
        string eventsCollectionName = $"{collectionPrefix}events";
        string snapshotsCollectionName = $"{collectionPrefix}snapshots";
        string outboxCollectionName = $$"""{{collectionPrefix}}{0}outbox""";

        var existingCollections = _db.ListCollectionNames().ToList();
        //if (!existingCollections.Contains(CollectionById))
        //{
        //    _db.CreateCollection(CollectionById);
        //}
        //if (!existingCollections.Contains(CollectionComposed))
        //{
        //    _db.CreateCollection(CollectionComposed);
        //}

        _logger = logger;
    }

    //private async Task CreateCollections()
    //{
    //    var unique = new CreateIndexOptions { Unique = true };

    //    var eventsPK = Builders<BsonDocument>.IndexKeys
    //        .Ascending(toSnakeCase(nameof(EvDbEventRecord.Domain)))
    //        .Ascending(toSnakeCase(nameof(EvDbEventRecord.Partition)))
    //        .Ascending(toSnakeCase(nameof(EvDbEventRecord.StreamId)))
    //        .Ascending(toSnakeCase(nameof(EvDbEventRecord.Offset)));
    //    var eventsIndexModel = new CreateIndexModel<BsonDocument>(eventsPK, unique);
    //    await _eventsCollection.Indexes.CreateOneAsync(eventsIndexModel);

    //    var outboxPK = Builders<BsonDocument>.IndexKeys
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Domain)))
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Partition)))
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.StreamId)))
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Offset)))
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.Channel)))
    //        .Ascending(toSnakeCase(nameof(EvDbMessageRecord.MessageType)));
    //    var outboxIndexModel = new CreateIndexModel<BsonDocument>(outboxPK, unique);
    //    await _outboxCollection.Indexes.CreateOneAsync(outboxIndexModel);

    //    var snapshotPK = Builders<BsonDocument>.IndexKeys
    //        .Ascending(toSnakeCase(nameof(EvDbViewAddress.Domain)))
    //        .Ascending(toSnakeCase(nameof(EvDbViewAddress.Partition)))
    //        .Ascending(toSnakeCase(nameof(EvDbViewAddress.StreamId)))
    //        .Ascending(toSnakeCase(nameof(EvDbViewAddress.ViewName)))
    //        .Ascending(toSnakeCase(nameof(EvDbStoredSnapshot.Offset)));
    //    var snapshotIndexModel = new CreateIndexModel<BsonDocument>(snapshotPK, unique);
    //    await _outboxCollection.Indexes.CreateOneAsync(snapshotIndexModel);
    //}


    EvDbMigrationQueryTemplates IEvDbStorageAdmin.Scripts => throw new NotImplementedException();

    Task IEvDbStorageAdmin.CreateEnvironmentAsync(CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    Task IEvDbStorageAdmin.DestroyEnvironmentAsync(CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

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

    ~MongoStorageAdmin()
    {
        DisposeAction();
    }

    #endregion //  DisposeAction Pattern
}

