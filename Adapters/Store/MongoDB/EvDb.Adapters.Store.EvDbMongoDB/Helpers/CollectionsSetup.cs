using EvDb.Adapters.Store.Internals;
using EvDb.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;

namespace EvDb.Adapters.Store.MongoDB.Internals;

public sealed class CollectionsSetup : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The outbox suffx
    /// </summary>
    private const string OUTBOX_SUFFX = "outbox";
    private readonly ILogger _logger;
    private readonly MongoClient _client;
    private readonly EvDbStorageContext _storageContext;
    private readonly EvDbMongoDBCreationMode _creationMode;
    private readonly string _collectionPrefix;
    private readonly IMongoDatabase _db;
    private readonly string _outboxCollectionFormat;
    private static readonly ConcurrentDictionary<CollectionIdentity, object?> _isShardedCache = new ConcurrentDictionary<CollectionIdentity, object?>();
    private readonly ConcurrentDictionary<string, object?> _isCollectionCreated = new ConcurrentDictionary<string, object?>();
    private static readonly SemaphoreSlim _collectionCreationSync = new SemaphoreSlim(1);
    private static readonly SemaphoreSlim _shardsCreationSync = new SemaphoreSlim(1);
    private static readonly CreateOneIndexOptions CREATE_INDEX_MAJORITY_OPTIONS = new CreateOneIndexOptions
    {
        CommitQuorum = CreateIndexCommitQuorum.Majority
        // MaxTime
    };
    private static readonly CreateOneIndexOptions CREATE_INDEX_SNAPSHOT_OPTIONS = new CreateOneIndexOptions
    {
        // MaxTime
    };
    private static readonly TimeSpan SETUP_TIMEOUT = TimeSpan.FromMinutes(2);

    #region Create

    /// <summary>
    /// Creates the specified logger.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="client">The client.</param>
    /// <param name="context">The context.</param>
    /// <param name="creationMode">The creation mode.</param>
    /// <returns></returns>
    public static CollectionsSetup Create(ILogger logger,
                                          MongoClient client,
                                          EvDbStorageContext context,
                                          EvDbMongoDBCreationMode creationMode)
    {
        return new CollectionsSetup(logger, client, context, creationMode);
    }

    #endregion //  Create

    #region Ctor

    private CollectionsSetup(
                    ILogger logger,
                    MongoClient client,
                    EvDbStorageContext storageContext,
                    EvDbMongoDBCreationMode creationMode)
    {
        _logger = logger;
        _client = client;
        _storageContext = storageContext;
        _creationMode = creationMode;
        _collectionPrefix = storageContext.CalcCollectionPrefix();
        _db = _client.GetDatabase(storageContext.DatabaseName);
        _outboxCollectionFormat = $$"""{{_collectionPrefix}}{0}{1}{{OUTBOX_SUFFX}}""";
        EventsCollectionTask = CreateEventsCollectionAsync();
        SnapshotsCollectionTask = CreateSnapshotsCollectionAsync();
    }

    #endregion //  Ctor

    public IMongoDatabase Db => _db;
    public Task<IMongoCollection<BsonDocument>> EventsCollectionTask { get; }
    public Task<IMongoCollection<BsonDocument>> SnapshotsCollectionTask { get; }
    private static readonly SemaphoreSlim _sync = new SemaphoreSlim(1);

    #region CreateEventsCollectionAsync

    public async Task<IMongoCollection<BsonDocument>> CreateEventsCollectionAsync(
                                        CancellationToken cancellationToken = default)
    {
        using var cts = new CancellationTokenSource(SETUP_TIMEOUT);

        string collectionName = $"{_collectionPrefix}events";

        if (_creationMode == EvDbMongoDBCreationMode.None)
        {
            return _db.GetCollection<BsonDocument>(collectionName, QueryProvider.EventsCollectionSetting);
        }
        CreateCollectionOptions options = new();
        bool exists = await CreateCollectionIfNotExistsAsync(collectionName,
                                                       options,
                                                       cancellationToken);

        IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName,
                                                             QueryProvider.EventsCollectionSetting);

        if (exists)
            return collection;

        foreach (var index in QueryProvider.EventsIndexes)
        {
            await collection.Indexes.CreateOneAsync(index,
                                                    CREATE_INDEX_MAJORITY_OPTIONS,
                                                    cts.Token);
        }
        try
        {
            await _sync.WaitAsync();
            await _client.ConfigureShardingAsync(_storageContext.DatabaseName, collectionName, QueryProvider.Sharding, _logger);
        }
        finally
        {
            _sync.Release();
        }

        await CreateShardingStrategyIfNotExistsAsync(_storageContext.DatabaseName, collectionName, cts.Token);

        return collection;
    }

    #endregion //  CreateEventsCollectionAsync

    #region CreateOutboxCollectionIfNotExistsAsync

    public async Task<IMongoCollection<BsonDocument>> CreateOutboxCollectionIfNotExistsAsync(
                                                            EvDbShardName shardName,
                                                            CancellationToken cancellation = default)
    {
        string separator = "_";
        if (string.Compare(shardName.Value, OUTBOX_SUFFX, true) == 0)
            shardName = string.Empty;
        if(string.IsNullOrEmpty(shardName.Value))
            separator = "";
        string collectionName = string.Format(_outboxCollectionFormat, shardName, separator);
        IMongoCollection<BsonDocument> outboxCollection =
                            await CreateOutboxCollectionIfNotExistsAsync(collectionName, cancellation);
        return outboxCollection;
    }

    private async Task<IMongoCollection<BsonDocument>> CreateOutboxCollectionIfNotExistsAsync(
                                                                string collectionName,
                                                                CancellationToken cancellationToken = default)
    {
        using var ctsWithTimeout = new CancellationTokenSource(SETUP_TIMEOUT);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ctsWithTimeout.Token);

        CreateCollectionOptions options = new CreateCollectionOptions
        {
        };

        var exists = await CreateCollectionIfNotExistsAsync(collectionName,
                                                           options,
                                                           cts.Token);
        var collection = _db.GetCollection<BsonDocument>(collectionName,
                                         QueryProvider.OutboxCollectionSetting);

        if (exists)
            return collection;
        foreach (var outboxIndex in QueryProvider.OutboxIndexes)
        {
            await collection.Indexes.CreateOneAsync(outboxIndex,
                                                    CREATE_INDEX_MAJORITY_OPTIONS,
                                                    cts.Token);
        }

        await CreateShardingStrategyIfNotExistsAsync(_storageContext.DatabaseName, collectionName, cts.Token);

        return collection;
    }

    #endregion //  CreateOutboxCollectionIfNotExistsAsync

    #region CreateSnapshotsCollectionAsync

    public async Task<IMongoCollection<BsonDocument>> CreateSnapshotsCollectionAsync(
                                            CancellationToken cancellationToken = default)
    {
        string collectionName = $"{_collectionPrefix}snapshots";
        CreateCollectionOptions options = new ();
        bool exists = await CreateCollectionIfNotExistsAsync(collectionName,
                                                   options,
                                                   cancellationToken);

        var collection = _db.GetCollection<BsonDocument>(collectionName,
                                                 QueryProvider.SnapshotCollectionSetting);
        if (exists)
            return collection;


        using var cts = new CancellationTokenSource(SETUP_TIMEOUT);

        foreach (var index in QueryProvider.SnapshotIndexes)
        {
            await collection.Indexes.CreateOneAsync(index,
                                                    CREATE_INDEX_SNAPSHOT_OPTIONS,
                                                    cts.Token);
        }

        await CreateShardingStrategyIfNotExistsAsync(_storageContext.DatabaseName, collectionName, cts.Token);

        return collection;
    }

    #endregion //  CreateSnapshotsCollectionAsync

    #region CreateShardingStrategyIfNotExistsAsync

    private async Task<bool> CreateShardingStrategyIfNotExistsAsync(
                                                    string databaseName,
                                                    string collectionName,
                                                    CancellationToken cancellationToken)
    {
        var collectionIdentity = new CollectionIdentity(databaseName, collectionName);
        bool succeed = await CreateShardingStrategyIfNotExistsAsync(collectionIdentity, cancellationToken);
        return succeed;
    }

    private async Task<bool> CreateShardingStrategyIfNotExistsAsync(
                                        CollectionIdentity collectionIdentity,
                                        CancellationToken cancellationToken)
    {
        #region return if exists

        if (_isShardedCache.ContainsKey(collectionIdentity))
            return true;
        if (await IsShardedAsync(collectionIdentity))
        {
            _isShardedCache.TryAdd(collectionIdentity, true);
            return true;
        }

        #endregion //  return if exists

        try
        {
            await _shardsCreationSync.WaitAsync(cancellationToken);

            #region return if exists

            if (_isShardedCache.ContainsKey(collectionIdentity))
                return true;

            #endregion //  return if exists

            bool succeed = await _client.ConfigureShardingAsync(collectionIdentity, QueryProvider.Sharding, _logger);
            if (succeed)
                _isShardedCache.TryAdd(collectionIdentity, null);
            return succeed;
        }
        finally
        {
            _shardsCreationSync.Release();
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

        // Extract the first matching existingCollection (if any)
        var collectionDoc = result["cursor"]["firstBatch"].AsBsonArray.FirstOrDefault();

        // Check if the existingCollection exists and is sharded
        bool isSharded = collectionDoc?.AsBsonDocument?.Contains("sharded") == true;
        return isSharded;
    }

    #endregion //  IsShardedAsync

    #region CreateCollectionIfNotExistsAsync

    private async Task<bool> CreateCollectionIfNotExistsAsync(
        string collectionName,
        CreateCollectionOptions options,
        CancellationToken cancellationToken = default)
    {
        if (_isCollectionCreated.TryGetValue(collectionName, out _))
            return true;
        try
        {
            await _collectionCreationSync.WaitAsync();
            if (_isCollectionCreated.TryGetValue(collectionName, out _))
                return true;

            // Query the list of collections and filter by name
            var filter = new BsonDocument("name", collectionName);
            IAsyncCursor<BsonDocument> collections = await _db.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            bool exists = await collections.AnyAsync();
            if (!exists)
            {
                await _db.CreateCollectionAsync(collectionName, options, cancellationToken);
            }

            _isCollectionCreated.TryAdd(collectionName, null);
            return false;
        }
        finally
        {
            _collectionCreationSync.Release();
        }
    }

    #endregion //  CreateCollectionIfNotExistsAsync

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
