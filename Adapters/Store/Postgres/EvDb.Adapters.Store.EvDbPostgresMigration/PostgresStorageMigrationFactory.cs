// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql

using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.Postgres;

public static class PostgresStorageMigrationFactory
{
    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        params EvDbShardName[] shardNames)
    {
        IEvDbStorageMigration result =
            new PostgresStorageMigration(
                    logger,
                    context,
                    factory,
                    StorageFeatures.All,
                    shardNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        IEvDbStorageMigration result =
            new PostgresStorageMigration(
                    logger,
                    context,
                    factory,
                    features,
                    shardNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        params EvDbShardName[] shardNames)
    {
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = Create(logger, factory, context, shardNames);
        return result;
    }

    public static IEvDbStorageMigration Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = Create(logger, factory, context, features, shardNames);
        return result;
    }

    public static EvDbMigrationQueryTemplates CreateScripts(
    ILogger logger,
    EvDbStorageContext context,
    StorageFeatures features,
    params EvDbShardName[] shardNames)
    {
        EvDbMigrationQueryTemplates scripts = QueryProvider.Create(context, features, shardNames);
        return scripts;
    }
}

