// Ignore Spelling: Mongo
// Ignore Spelling: Admin

using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB;

public static class MongoDBStorageAdminFactory
{
    public static IEvDbStorageAdmin Create(ILogger logger,
                                             string connectionString,
                                             EvDbStorageContext ctx,
                                             EvDbShardName[] shardNames)
    {
        MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
        return Create(logger, settings, ctx, shardNames);
    }

    public static IEvDbStorageAdmin Create(ILogger logger,
                                             MongoClientSettings settings,
                                             EvDbStorageContext ctx,
                                             EvDbShardName[] shardNames)
    {
        return Create(logger, settings, ctx, StorageFeatures.All, shardNames);
    }

    public static IEvDbStorageAdmin Create(ILogger logger,
                                             MongoClientSettings settings,
                                             EvDbStorageContext ctx,
                                             StorageFeatures features,
                                             EvDbShardName[] shardNames)
    {
        return new MongoDBStorageAdmin(logger, settings, ctx, features, shardNames);
    }
}

