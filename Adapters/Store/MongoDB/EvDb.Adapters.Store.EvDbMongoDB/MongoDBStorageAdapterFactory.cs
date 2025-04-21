// Ignore Spelling: MongoDB Mongo

using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB;


public static class EvDbMongoDBStorageAdapterFactory
{
    
    #region CreateStreamAdapter

    #region Overload

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
                        ILogger logger,
                        string connectionString,
                        EvDbStorageContext context,
                        IEnumerable<IEvDbOutboxTransformer> transformers,
                        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
    {
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
        var result = CreateStreamAdapter(logger, settings, context, transformers, creationMode);
        return result;
    }

    #endregion //  Overload

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        MongoClientSettings settings,
        EvDbStorageContext context,
        IEnumerable<IEvDbOutboxTransformer> transformers,
        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
    {
        IEvDbStorageStreamAdapter result = new EvDbMongoDBStorageAdapter(
                    settings,
                    logger,
                    context,
                    transformers,
                    creationMode);
        return result;
    }

    #endregion //  CreateStreamAdapter

    #region CreateSnapshotAdapter

    #region Overload

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
                        ILogger logger,
                        string connectionString,
                        EvDbStorageContext context,
                        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
    {
        MongoClientSettings settings = MongoClientSettings.FromConnectionString(connectionString);
        var result = CreateSnapshotAdapter(logger, settings, context, creationMode);
        return result;
    }

    #endregion //  Overload

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        MongoClientSettings settings,
        EvDbStorageContext context,
        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
    {
        IEvDbStorageSnapshotAdapter result = new EvDbMongoDBStorageAdapter(
                    settings,
                    logger,
                    context,
                    [],
                    creationMode);
        return result;
    }

    #endregion //  CreateSnapshotAdapter
}
