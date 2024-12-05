// Ignore Spelling: Postgres

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.Postgres;

public static class EvDbPostgresStorageAdapterFactory
{
    #region Ctor

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        IEnumerable<IEvDbOutboxTransformer> transformers)
    {
        IEvDbStorageStreamAdapter result = new EvDbPostgresStorageAdapter(
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
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = CreateStreamAdapter(logger, factory, context, transformers);
        return result;
    }

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageSnapshotAdapter result = new EvDbPostgresStorageAdapter(
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
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = CreateSnapshotAdapter(logger, factory, context);
        return result;
    }

    #endregion // Ctor
}
