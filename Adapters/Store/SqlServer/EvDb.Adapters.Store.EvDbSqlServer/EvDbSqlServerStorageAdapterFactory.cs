using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

public static class EvDbSqlServerStorageAdapterFactory
{
    #region Ctor

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageStreamAdapter result = new EvDbSqlServerStorageAdapter(
                    logger,
                    context,
                    factory);
        return result;
    }

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = CreateStreamAdapter(logger, factory, context);
        return result;
    }

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageSnapshotAdapter result = new EvDbSqlServerStorageAdapter(
                    logger,
                    context,
                    factory);
        return result;
    }

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = CreateSnapshotAdapter(logger, factory, context);
        return result;
    }

    #endregion // Ctor
}
