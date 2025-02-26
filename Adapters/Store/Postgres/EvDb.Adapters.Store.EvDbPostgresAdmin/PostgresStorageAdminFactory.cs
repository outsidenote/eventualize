// Ignore Spelling: Sql
// Ignore Spelling: Admin

using EvDb.Core;

using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.Postgres;

public static class PostgresStorageAdminFactory
{
    #region Overloads

    public static IEvDbStorageAdmin Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        params EvDbShardName[] shardNames)
    {
        return Create(logger, factory, context, StorageFeatures.All, shardNames);
    }

    public static IEvDbStorageAdmin Create(
        ILogger logger,
        string connectionString,
        EvDbStorageContext context,
        params EvDbShardName[] shardNames)
    {
        IEvDbConnectionFactory factory = new EvDbPostgresConnectionFactory(connectionString);
        var result = Create(logger, factory, context, shardNames);
        return result;
    }

    public static IEvDbStorageAdmin Create(
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

    #endregion //  Overloads

    public static IEvDbStorageAdmin Create(
        ILogger logger,
        IEvDbConnectionFactory factory,
        EvDbStorageContext context,
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        var scripting = PostgresStorageScripting.Default;
        var adminFactory = new EvDbRelationalStorageAdminFactory(logger, factory, scripting);
        IEvDbStorageAdmin result = adminFactory.Create(context, features, shardNames);
        return result;
    }
}

