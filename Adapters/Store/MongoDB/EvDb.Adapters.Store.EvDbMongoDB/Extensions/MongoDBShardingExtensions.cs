using EvDb.Adapters.Store.EvDbMongoDB.Internals;
using global::MongoDB.Bson;
using global::MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store;

// https://claude.ai/share/076cd430-53ea-4149-9ffb-549331451dc4

internal static class MongoDBShardingExtensions
{
    #region ConfigureShardingAsync

    public static async Task<bool> ConfigureShardingAsync(this MongoClient adminClient,
                                                    string databaseName,
                                                    string collectionName,
                                                    BsonDocument shardKeys,
                                                    ILogger logger)
    {
        var collectionIdentity = new CollectionIdentity(databaseName, collectionName);
        var result = await adminClient.ConfigureShardingAsync(collectionIdentity, shardKeys, logger);
        return result;
    }

    public static async Task<bool> ConfigureShardingAsync(this MongoClient adminClient,
                                                    CollectionIdentity collectionIdentity,
                                                    BsonDocument shardKeys,
                                                    ILogger logger)
    {
        (string databaseName, string collectionName) = collectionIdentity;
        // 1. Enable sharding for the database
        var adminDb = adminClient.GetDatabase("admin");

        // 2. check if sharding supported
        bool shrdingSupported = await adminDb.DoesSupportSharding();
        if (!shrdingSupported)
            return true;

        var enableShardingCommand = QueryProvider.CreateEnableShardingCommand(databaseName);

        await adminDb.RunCommandAsync<BsonDocument>(enableShardingCommand);

        // 3. Shard the collection
        var shardCollectionCommand = new BsonDocument
            {
                { "shardCollection", $"{databaseName}.{collectionName}" },
                { "key", shardKeys }
            };

        BsonDocument shardResult = await adminDb.RunCommandAsync<BsonDocument>(shardCollectionCommand);
        
        // TODO: [bnaya, 2025-03-12] high perf logging
        logger.LogInformation("Sharding [{DatabaseName}:{CollectionName}]: {Sharding}", databaseName, collectionName, shardResult.ToJson());
        return true;
    }

    #endregion //  ConfigureShardingAsync

    #region DoesSupportSharding

    private static async Task<bool> DoesSupportSharding(this IMongoDatabase adminDb)
    {
        var helloCommand = new BsonDocument("hello", 1);
        var result = await adminDb.RunCommandAsync<BsonDocument>(helloCommand);

        // Check if the server is a mongos router (indicating a sharded cluster)
        bool isMongos = result.Contains("msg") && result["msg"] == "isdbgrid";
        return isMongos;
    }

    #endregion //  DoesSupportSharding
}