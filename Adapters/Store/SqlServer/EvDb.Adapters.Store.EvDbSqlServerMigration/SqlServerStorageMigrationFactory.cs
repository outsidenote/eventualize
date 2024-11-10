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
        params EvDbShardName[] topicShardNames)
    {
        IEvDbStorageMigration result =
            new SqlServerStorageMigration(
                    logger,
                    context,
                    factory,
                    topicShardNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        params EvDbShardName[] topicShardNames)
    {
        IEvDbConnectionFactory factory = new EvDbSqlConnectionFactory(connectionString);
        var result = Create(logger, factory, context, topicShardNames);
        return result;
    }
}

