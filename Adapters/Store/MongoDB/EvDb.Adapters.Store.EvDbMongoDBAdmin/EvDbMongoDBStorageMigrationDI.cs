// Ignore Spelling: Mongo

using EvDb.Adapters.Store.MongoDB;
using EvDb.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbMongoDBStorageMigrationDI
{
    #region Overloads

    public static IServiceCollection AddEvDbMongoDBStoreAdmin(
            this IServiceCollection services,
            string connectionStringOrKey,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbMongoDBStoreAdmin(
                            null,
                            connectionStringOrKey,
                            shardNames);
    }

    public static IServiceCollection AddEvDbMongoDBStoreAdmin(
            this IServiceCollection services,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbMongoDBStoreAdmin(
                            null,
                            "EvDbMongoDBConnection",
                            shardNames);
    }

    #endregion //  Overloads

    public static IServiceCollection AddEvDbMongoDBStoreAdmin(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbPostgresConnection",
            params EvDbShardName[] shardNames)
    {
        services.AddSingleton<IEvDbStorageAdminScripting, MongoStorageScripting>();
        services.AddSingleton(sp =>
        {
            var ctx = context
                ?? sp.GetService<EvDbStorageContext>()
                ?? EvDbStorageContext.CreateWithEnvironment("evdb");

            #region IEvDbConnectionFactory connectionFactory = ...

            string connectionString;
            IConfiguration? configuration = sp.GetService<IConfiguration>();
            connectionString = configuration?.GetConnectionString(connectionStringOrKey) ?? connectionStringOrKey;

            #endregion // IEvDbConnectionFactory connectionFactory = ...

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MongoDBStorageAdmin>();
            IEvDbStorageAdmin adapter = MongoDBStorageAdminFactory.Create(logger, connectionString, ctx, shardNames);
            return adapter;
        });

        return services;
    }
}
