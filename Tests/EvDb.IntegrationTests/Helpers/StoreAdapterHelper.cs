using EvDb.Adapters.Store.MongoDB;
using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core.Internals;
using EvDb.Core.Store.Internals;
using FakeItEasy;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace EvDb.Core.Tests;

public static class StoreAdapterHelper
{
    #region ChooseStoreAdapter

    public static EvDbStreamStoreRegistrationContext ChooseStoreAdapter(
                                        this EvDbStreamStoreRegistrationContext context,
                                        StoreType storeType,
                                        EvDbStreamTestingStorage testingStore)
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
                context.UseMongoDBStoreForEvDbStream(EvDbMongoDBCreationMode.None);
                break;
            case StoreType.Testing:
                context.UseTestingStoreForEvDbStream(testingStore);
                break;
            default:
                throw new NotImplementedException();
        }

        return context;
    }

    #endregion //  ChooseStoreAdapter

    #region AddChangeStream

    public static IServiceCollection AddChangeStream(
                                        this IEvDbRegistrationEntry registration,
                                        StoreType storeType,
                                        EvDbStorageContext context)
    {
        switch (storeType)
        {
            case StoreType.SqlServer:
                registration.Services.UseSqlServerChangeStream(context);
                break;
            case StoreType.Postgres:
                registration.Services.UsePostgresChangeStream(context);
                break;
            case StoreType.MongoDB:
                registration.Services.UseMongoDBChangeStream(context);
                break;
            case StoreType.Testing:
                break;
            default:
                throw new NotImplementedException();
        }

        return registration.Services;
    }

    #endregion //  AddChangeStream

    #region ChooseSnapshotAdapter

    public static EvDbSnapshotStoreRegistrationContext ChooseSnapshotAdapter(
        this EvDbSnapshotStoreRegistrationContext context,
        StoreType storeType,
        EvDbStreamTestingStorage testingStore,
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
            case StoreType.Testing:
                context.UseTestingForEvDbSnapshot(overrideContext, testingStore);
                break;
            default:
                throw new NotImplementedException();
        }
        return context;
    }

    #endregion //  ChooseSnapshotAdapter

    #region GetConnection

    public static DbConnection GetConnection(StoreType storeType,
        EvDbStorageContext storageContext)
    {
        string connectionString = GetConnectionString(storeType);
        DbConnection conn = storeType switch
        {
            StoreType.SqlServer => new SqlConnection(connectionString),
            StoreType.Postgres => new NpgsqlConnection(connectionString),
            //StoreType.MongoDB => ,
            //StoreType.Testing => ,
            _ => throw new NotImplementedException()
        };

        return conn;
    }

    #endregion //  GetConnection

    #region CreateStoreMigration

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
            // StoreType.MongoDB => "root",
            // StoreType.Testing => "root",
            _ => EvDbSchemaName.Empty
        };
        EvDbDatabaseName dbName = storeType switch
        {
            StoreType.SqlServer => "master",
            StoreType.Postgres => "tests",
            StoreType.MongoDB => "tests",
            // StoreType.Testing => "tests",
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
            StoreType.MongoDB =>
                MongoDBStorageAdminFactory.Create(logger, connectionString, context, shardNames),
            StoreType.Testing => A.Fake<IEvDbStorageAdmin>(),
            _ => throw new NotImplementedException()
        };

        return result;
    }

    #endregion //  CreateStoreMigration

    #region GetConnectionString

    public static string GetConnectionString(this StoreType storeType)
    {
        if (storeType == StoreType.Testing)
            return string.Empty;

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

    #endregion //  GetConnectionString
}
