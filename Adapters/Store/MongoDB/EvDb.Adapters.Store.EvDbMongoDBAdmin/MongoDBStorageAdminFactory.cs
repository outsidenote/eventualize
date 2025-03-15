// Ignore Spelling: Mongo
// Ignore Spelling: Admin

using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB;

public class MongoDBStorageAdminFactory
{
    //public static readonly IEvDbStorageAdminFactory Create()
    //{
    //    new MongoStorageAdminFactory()  
    //}

    public MongoDBStorageAdminFactory()
    {

    }

    internal static IEvDbStorageAdmin Create(ILogger<MongoDBStorageAdminFactory> logger,
                                             string connectionString,
                                             EvDbStorageContext ctx,
                                             EvDbShardName[] shardNames)
    {
        var settings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
        return Create(logger, settings, ctx, shardNames);
    }

    internal static IEvDbStorageAdmin Create(ILogger<MongoDBStorageAdminFactory> logger,
                                             MongoClientSettings settings,
                                             EvDbStorageContext ctx,
                                             EvDbShardName[] shardNames)
    {
        return Create(logger, settings, ctx, StorageFeatures.All, shardNames);
    }

    internal static IEvDbStorageAdmin Create(ILogger<MongoDBStorageAdminFactory> logger,
                                             MongoClientSettings settings,
                                             EvDbStorageContext ctx,
                                             StorageFeatures features,
                                             EvDbShardName[] shardNames)
    {
        return new MongoDBStorageAdmin(logger, settings, ctx, features, shardNames);
    }

    public IEvDbStorageAdmin Create(EvDbStorageContext context,
                                                      StorageFeatures features,
                                                      params EvDbShardName[] shardNames)
    {
        throw new NotImplementedException();
    }


}

