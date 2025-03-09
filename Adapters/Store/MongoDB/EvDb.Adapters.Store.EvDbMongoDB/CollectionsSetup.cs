using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Threading;

namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

internal sealed class CollectionsSetup: IDisposable, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly MongoClient _client;
    private readonly EvDbStorageContext _storageContext;
    private readonly string _collectionPrefix;
    private readonly IMongoDatabase _db;
    private readonly string _outboxCollectionFormat;
    private static readonly ConcurrentDictionary<CollectionIdentity, bool> _isShardedCache = new ConcurrentDictionary<CollectionIdentity, bool>();
    private readonly ConcurrentDictionary<EvDbShardName, IMongoCollection<BsonDocument>> _outboxCollections = new ConcurrentDictionary<EvDbShardName, IMongoCollection<BsonDocument>>();
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    private static readonly CreateOneIndexOptions CREATE_INDEX_OPTIONS = new CreateOneIndexOptions
    {
        CommitQuorum = CreateIndexCommitQuorum.Majority,
    };
    public static CollectionsSetup Create(ILogger logger, MongoClient client, EvDbStorageContext context) => new CollectionsSetup(logger, client, context);

    #region Ctor

    private CollectionsSetup(
                    ILogger logger,
                    MongoClient client,
                    EvDbStorageContext storageContext)
    {
        _logger = logger;
        _client = client;
        _storageContext = storageContext;
        _collectionPrefix = storageContext.CalcCollectionPrefix();
        _db = _client.GetDatabase(storageContext.DatabaseName);
        _outboxCollectionFormat = $$"""{{_collectionPrefix}}{0}outbox""";
    }

    #endregion //  Ctor

    public IMongoDatabase Db => _db;

    #region CreateEventsCollectionAsync

    public async Task<IMongoCollection<BsonDocument>> CreateEventsCollectionAsync(
                                                        CancellationToken cancellationToken)
    {
        string collectionName = $"{_collectionPrefix}events";
        var collection = _db.GetCollection<BsonDocument>(collectionName,
                                                         QueryProvider.EventsCollectionSetting);
        await collection.Indexes.CreateOneAsync(QueryProvider.EventsPK,
                                                CREATE_INDEX_OPTIONS,
                                                cancellationToken);

        var collectionIdentity = new CollectionIdentity(_storageContext.DatabaseName, collectionName);
        await CreateShardingStrategyIfNotExistsAsync(collectionIdentity, cancellationToken);

        return collection;
    }

    #endregion //  CreateEventsCollectionAsync

    #region CreateSnapshotsCollectionAsync

    public async Task<IMongoCollection<BsonDocument>> CreateSnapshotsCollectionAsync(
                                                        CancellationToken cancellationToken)
    {
        string collectionName = $"{_collectionPrefix}snapshots";
        var collection = _db.GetCollection<BsonDocument>(
                                    collectionName,
                                    QueryProvider.SnapshotCollectionSetting);
        await collection.Indexes.CreateOneAsync(QueryProvider.SnapshotPK,
                                                CREATE_INDEX_OPTIONS,
                                                cancellationToken);

        var collectionIdentity = new CollectionIdentity(_storageContext.DatabaseName, collectionName);
        await CreateShardingStrategyIfNotExistsAsync(collectionIdentity, cancellationToken);

        return collection;
    }

    #endregion //  CreateSnapshotsCollectionAsync

    #region CreateShardingStrategyIfNotExistsAsync

    private async Task CreateShardingStrategyIfNotExistsAsync(
                                        CollectionIdentity collectionIdentity,
                                        CancellationToken cancellationToken)
    {
        #region return if exists

        if (_isShardedCache.ContainsKey(collectionIdentity))
            return;
        if (await IsShardedAsync(collectionIdentity))
        {
            _isShardedCache.TryAdd(collectionIdentity, true);
            return;
        }

        #endregion //  return if exists

        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            #region return if exists

            if (_isShardedCache.ContainsKey(collectionIdentity))
                return;

            #endregion //  return if exists

            var adminDb = _client.GetDatabase("admin");
            BsonDocument shardCommand = Commands.CreateEventsShardCommand(collectionIdentity);
            await adminDb.RunCommandAsync<BsonDocument>(shardCommand);

            // TODO: [bnaya 2025-03-09] hi perf logging
            _logger.LogInformation($"Collection '{collectionIdentity}' sharded successfully with key {shardCommand.ToJson()}.");
        }
        finally
        {
            _semaphore.Release();
        }
    }

    #endregion //  CreateShardingStrategyIfNotExistsAsync

    #region IsShardedAsync

    private async Task<bool> IsShardedAsync(CollectionIdentity collectionIdentity)
    {
        IMongoDatabase adminDb = _client.GetDatabase(collectionIdentity.DatabaseName);
        // Define a filtered `listCollections` command

        var listCollectionsCommand = Commands.CreateListCollectionCommand(collectionIdentity);
        // Execute the command and return only relevant collections
        BsonDocument result = await adminDb.RunCommandAsync<BsonDocument>(listCollectionsCommand);

        // Extract the first matching collection (if any)
        var collectionDoc = result["cursor"]["firstBatch"].AsBsonArray.FirstOrDefault();

        // Check if the collection exists and is sharded
        bool isSharded = collectionDoc?.AsBsonDocument?.Contains("sharded") == true;
        return isSharded;
    }

    #endregion //  IsShardedAsync

    #region CreateOutboxCollectionAsync

    public async Task<IMongoCollection<BsonDocument>> GetOutboxCollectionAsync(EvDbShardName shardName, CancellationToken cancellationToken)
    {
        #region return if exists

        if (_outboxCollections.TryGetValue(shardName, out IMongoCollection<BsonDocument>? collection))
            return collection;

        #endregion //  return if exists

        try
        {
            #region return if exists

            if (_outboxCollections.TryGetValue(shardName, out collection))
                return collection;

            #endregion //  return if exists

            IMongoCollection<BsonDocument> outboxCollection =
                                await CreateOutboxCollectionAsync(shardName, cancellationToken);
            _outboxCollections.TryAdd(shardName, outboxCollection);
            return outboxCollection;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    private async Task<IMongoCollection<BsonDocument>> CreateOutboxCollectionAsync(EvDbShardName shardName, CancellationToken cancellationToken)
    {
        string collectionName = string.Format(_outboxCollectionFormat, shardName);
        var collection = _db.GetCollection<BsonDocument>(collectionName,
                                                         QueryProvider.EventsCollectionSetting);


        await collection.Indexes.CreateOneAsync(QueryProvider.EventsPK,
                                                CREATE_INDEX_OPTIONS,
                                                cancellationToken);

        var collectionIdentity = new CollectionIdentity(_storageContext.DatabaseName, collectionName);
        await CreateShardingStrategyIfNotExistsAsync(collectionIdentity, cancellationToken);

        return collection;
    }

    #endregion //  CreateOutboxCollectionAsync

    #region Commands

    private static class Commands
    {
        #region CreateListCollectionCommand

        public static BsonDocument CreateListCollectionCommand(CollectionIdentity collectionIdentity)
        {
            var listCollectionsCommand = new BsonDocument
            {
                ["listCollections"] = 1,
                ["filter"] = new BsonDocument
                {
                    ["name"] = collectionIdentity.CollectionName
                }
            };
            return listCollectionsCommand;
        }

        #endregion //  CreateListCollectionCommand

        #region CreateEventsShardCommand

        public static BsonDocument CreateEventsShardCommand(CollectionIdentity collectionIdentity)
        {
            var fullCollectionName = collectionIdentity.ToString();

            var shardCommand = new BsonDocument
            {
                ["shardCollection"] = fullCollectionName,
                ["key"] = new BsonDocument
                {
                    [EvDbFileds.Event.Domain] = 1,
                    [EvDbFileds.Event.Partition] = 1,
                    [EvDbFileds.Event.EventType] = 1
                }
            };
            return shardCommand;
        }

        #endregion //  CreateEventsShardCommand
    }

    #endregion //  Commands

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
        _client.Dispose();
    }

    ~CollectionsSetup()
    {
        DisposeAction();
    }

    #endregion //  DisposeAction Pattern

}
