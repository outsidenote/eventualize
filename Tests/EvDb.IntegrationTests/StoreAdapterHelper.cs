using EvDb.Adapters.Store.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Data.SqlClient;

namespace EvDb.Core.Tests;

public record StoreAdapters(IEvDbStorageStreamAdapter Stream, IEvDbStorageSnapshotAdapter Snapshot);

public static class StoreAdapterHelper
{
    public static StoreAdapters CreateStoreAdapter(
        ILogger logger,
        StoreType storeType,
        EvDbTestStorageContext context)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        string connectionKey = storeType switch
        {
            StoreType.SqlServer => "EvDbSqlServerConnection",
            StoreType.Posgres => "EvDbPosgresConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);

        IEvDbStorageStreamAdapter streamStoreAdapter = storeType switch
        {
            StoreType.SqlServer =>
                EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []),
            //StoreType.Posgres => ,
            //    PosgresStorageAdapterFactory.Create(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };

        IEvDbStorageSnapshotAdapter snapshotStoreAdapter = storeType switch
        {
            StoreType.SqlServer =>
                EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, context),
            //StoreType.Posgres => ,
            //    PosgresStorageAdapterFactory.Create(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };
        return new StoreAdapters(streamStoreAdapter, snapshotStoreAdapter);
    }

    public static DbConnection GetConnection(StoreType storeType,
        EvDbTestStorageContext storageContext)
    {
        string connectionString = GetConnectionString(storeType);
        DbConnection conn = storeType switch
        {
            StoreType.SqlServer => new SqlConnection(connectionString),
            //StoreType.Posgres => ,
            _ => throw new NotImplementedException()
        };

        return conn;
    }

    public static IEvDbStorageMigration CreateStoreMigration(
        ILogger logger,
        StoreType storeType,
        EvDbTestStorageContext? context = null,
        params EvDbShardName[] shardNames)
    {
        context = context ?? new EvDbTestStorageContext();
        string connectionString = GetConnectionString(storeType);

        IEvDbStorageMigration result = storeType switch
        {
            StoreType.SqlServer =>
                SqlServerStorageMigrationFactory.Create(logger, connectionString, context, shardNames),
            //StoreType.Posgres => ,
            //    PosgresStorageAdapterFactory.Create(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };

        return result;
    }

    public static string GetConnectionString(StoreType storeType)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        string connectionKey = storeType switch
        {
            StoreType.SqlServer => "EvDbSqlServerConnection",
            StoreType.Posgres => "EvDbPosgresConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);
        return connectionString;
    }
}
