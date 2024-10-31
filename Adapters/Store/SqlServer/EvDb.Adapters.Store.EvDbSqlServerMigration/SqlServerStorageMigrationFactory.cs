// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql

using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

public static class SqlServerStorageMigrationFactory
{
    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        params string[] topicTableNames)
    {
        var result = Create(logger, factory, "master", context, topicTableNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        params string[] topicTableNames)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, context, topicTableNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        string dbName,
        EvDbStorageContext context,
        params string[] topicTableNames)
    {
        IEvDbStorageMigration result =
            new SqlServerStorageMigration(
                    logger,
                    dbName,
                    context,
                    factory,
                    topicTableNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        string dbName,
        EvDbStorageContext context,
        params string[] topicTableNames)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, dbName, context, topicTableNames);
        return result;
    }
}

