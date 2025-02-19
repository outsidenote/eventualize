// Ignore Spelling: MongoDB

using EvDb.Adapters.Store.MongoDB;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbMongoDBStorageMigrationDI
{
    public static IServiceCollection AddEvDbMongoDBStoreMigration(
            this IServiceCollection services,
            string connectionStringOrKey,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbMongoDBStoreMigration(
                            null,
                            connectionStringOrKey,
                            shardNames);
    }

    public static IServiceCollection AddEvDbMongoDBStoreMigration(
            this IServiceCollection services,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbMongoDBStoreMigration(
                            null,
                            "EvDbMongoDBConnection",
                            shardNames);
    }


    public static IServiceCollection AddEvDbMongoDBStoreMigration(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbMongoDBConnection",
            params EvDbShardName[] shardNames)
    {
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
            var logger = loggerFactory.CreateLogger<EvDbRelationalStorageMigration>();
            IEvDbStorageMigration adapter = MongoDBStorageMigrationFactory.Create(logger, connectionString, ctx, shardNames);
            return adapter;
        });

        return services;
    }
}
