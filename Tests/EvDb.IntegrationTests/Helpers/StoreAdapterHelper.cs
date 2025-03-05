using EvDb.Adapters.Store.MongoDB;
using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core.Store.Internals;
using FakeItEasy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace EvDb.Core.Tests;

public record StoreAdapters(IEvDbStorageStreamAdapter Stream, IEvDbStorageSnapshotAdapter Snapshot);

public static class StoreAdapterHelper
{

    public static EvDbStreamStoreRegistrationContext ChooseStoreAdapter(
                                        this EvDbStreamStoreRegistrationContext context,
                                        StoreType storeType)
    {
        switch (storeType)
        {
            case StoreType.SqlServer:
                context.UseSqlServerStoreForEvDbStream();
                break;
            case StoreType.Postgres:
                context.UsePostgresStoreForEvDbStream();
                break;
            case StoreType.MongoDB:
                context.UseMongoDBStoreForEvDbStream();
                break;
            default:
                throw new NotImplementedException();
        }

        return context;
    }

    public static EvDbSnapshotStoreRegistrationContext ChooseSnapshotAdapter(
        this EvDbSnapshotStoreRegistrationContext context,
        StoreType storeType,
        EvDbStorageContext? overrideContext = null)
    {
        switch (storeType)
        {
            case StoreType.SqlServer:
                context.UseSqlServerForEvDbSnapshot(overrideContext);
                break;
            case StoreType.Postgres:
                context.UsePostgresForEvDbSnapshot(overrideContext);
                break;
            case StoreType.MongoDB:
                context.UseMongoDBForEvDbSnapshot(overrideContext);
                break;
            default:
                throw new NotImplementedException();
        }
        return context;
    }


    public static StoreAdapters CreateStoreAdapter(
        this ILogger logger,
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
            StoreType.Postgres => "EvDbPostgresConnection",
            StoreType.MongoDB => "EvDbMongoDBConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);

        IEvDbStorageStreamAdapter streamStoreAdapter = storeType switch
        {
            StoreType.SqlServer =>
                EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []),
            StoreType.Postgres =>
                EvDbPostgresStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []),
            StoreType.MongoDB =>
                EvDbMongoDBStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []),
            _ => throw new NotImplementedException()
        };

        IEvDbStorageSnapshotAdapter snapshotStoreAdapter = storeType switch
        {
            StoreType.SqlServer =>
                EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, context),
            StoreType.Postgres =>
                EvDbPostgresStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, context),
            _ => throw new NotImplementedException()
        };
        return new StoreAdapters(streamStoreAdapter, snapshotStoreAdapter);
    }

    public static DbConnection GetConnection(StoreType storeType,
        EvDbStorageContext storageContext)
    {
        string connectionString = GetConnectionString(storeType);
        DbConnection conn = storeType switch
        {
            StoreType.SqlServer => new SqlConnection(connectionString),
            StoreType.Postgres => new NpgsqlConnection(connectionString),
            //StoreType.MongoDB => ,
            _ => throw new NotImplementedException()
        };

        return conn;
    }

    public static IEvDbStorageAdmin CreateStoreMigration(
        ILogger logger,
        StoreType storeType,
        EvDbStorageContext? context = null,
        params EvDbShardName[] shardNames)
    {
        EvDbSchemaName schema = storeType switch
        {
            StoreType.SqlServer => "dbo",
            StoreType.Postgres => "public",
            _ => EvDbSchemaName.Empty
        };
        EvDbDatabaseName dbName = storeType switch
        {
            StoreType.SqlServer => "master",
            StoreType.Postgres => "tests",
            _ => EvDbDatabaseName.Empty
        };
        context = context ?? new EvDbTestStorageContext(schema, dbName);
        string connectionString = GetConnectionString(storeType);

        IEvDbStorageAdmin result = storeType switch
        {
            StoreType.SqlServer =>
                SqlServerStorageAdminFactory.Create(logger, connectionString, context, shardNames),
            StoreType.Postgres =>
                PostgresStorageAdminFactory.Create(logger, connectionString, context, shardNames),
            StoreType.MongoDB => // TODO: [bnaya 2025-03-03] set it right 
                A.Fake<IEvDbStorageAdmin>(),
                //MongoDBStorageAdminFactory.Create(logger, connectionString, context, shardNames),
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
            StoreType.Postgres => "EvDbPostgresConnection",
            StoreType.MongoDB => "EvDbMongoDBConnection",
            _ => throw new NotImplementedException()
        };


        string connectionString = configuration.GetConnectionString(connectionKey) ?? throw new ArgumentNullException(connectionKey);
        return connectionString;
    }
}
