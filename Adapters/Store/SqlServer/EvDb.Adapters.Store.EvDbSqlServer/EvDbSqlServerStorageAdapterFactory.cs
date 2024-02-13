using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

public static class EvDbSqlServerStorageAdapterFactory
{
    #region Ctor

    public static IEvDbStorageAdapter Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageAdapter result = new EvDbSqlServerStorageAdapter(
                    logger,
                    context,
                    factory);
        return result;
    }

    public static IEvDbStorageAdapter Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, context);
        return result;
    }

    #endregion // Ctor
}
