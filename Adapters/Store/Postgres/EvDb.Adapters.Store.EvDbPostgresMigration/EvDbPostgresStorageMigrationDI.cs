﻿// Ignore Spelling: Postgres

using EvDb.Adapters.Store.Postgres;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbPostgresStorageMigrationDI
{
    public static IServiceCollection AddEvDbPostgresStoreMigration(
            this IServiceCollection services,
            string connectionStringOrKey,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbPostgresStoreMigration(
                            null,
                            connectionStringOrKey,
                            shardNames);
    }

    public static IServiceCollection AddEvDbPostgresStoreMigration(
            this IServiceCollection services,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbPostgresStoreMigration(
                            null,
                            "EvDbPostgresConnection",
                            shardNames);
    }


    public static IServiceCollection AddEvDbPostgresStoreMigration(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbPostgresConnection",
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
            IEvDbStorageMigration adapter = PostgresStorageMigrationFactory.Create(logger, connectionString, ctx, shardNames);
            return adapter;
        });

        return services;
    }
}
