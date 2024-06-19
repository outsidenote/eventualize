// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

public static class SqlServerStorageMigrationFactory
{
    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context)
    {
        IEvDbStorageMigration result =
            new SqlServerStorageMigration(
                    logger,
                    context,
                    factory);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, context);
        return result;
    }
}

