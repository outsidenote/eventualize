// Ignore Spelling: MongoDB

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.MongoDB;

public static class EvDbMongoDBStorageAdapterFactory
{
    #region Ctor

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        IEvDbStorageStreamAdapter result = new EvDbMongoDBStorageAdapter(
                    logger,
                    context,
                    factory,
                    transformers);
        return result;
    }

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        IEvDbConnectionFactory factory = new EvDbMongoDBConnectionFactory(connectionString);
        var result = CreateStreamAdapter(logger, factory, context, transformers);
        return result;
    }

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageSnapshotAdapter result = new EvDbMongoDBStorageAdapter(
                    logger,
                    context,
                    factory,
                    /*Snapshots don't need transformation*/[]);
        return result;
    }

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbMongoDBConnectionFactory(connectionString);
        var result = CreateSnapshotAdapter(logger, factory, context);
        return result;
    }

    #endregion // Ctor
}
