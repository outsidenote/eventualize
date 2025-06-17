// Ignore Spelling: Postgres

using EvDb.Adapters.Internals;
using EvDb.Adapters.Store.Postgres;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbPostgresStorageAdminDI
{
    #region Overloads

    public static IServiceCollection AddEvDbPostgresStoreAdmin(
            this IServiceCollection services,
            string connectionStringOrKey,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbPostgresStoreAdmin(
                            null,
                            connectionStringOrKey,
                            shardNames);
    }

    public static IServiceCollection AddEvDbPostgresStoreAdmin(
            this IServiceCollection services,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbPostgresStoreAdmin(
                            null,
                            "EvDbPostgresConnection",
                            shardNames);
    }

    #endregion //  Overloads

    public static IServiceCollection AddEvDbPostgresStoreAdmin(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbPostgresConnection",
            params EvDbShardName[] shardNames)
    {
        services.AddSingleton<IEvDbStorageAdminScripting, PostgresStorageScripting>();
        services.AddSingleton(sp =>
        {
            var ctx = sp.GetEvDbStorageContextFallback(context);

            #region IEvDbConnectionFactory connectionFactory = ...

            string connectionString;
            IConfiguration? configuration = sp.GetService<IConfiguration>();
            connectionString = configuration?.GetConnectionString(connectionStringOrKey) ?? connectionStringOrKey;

            #endregion // IEvDbConnectionFactory connectionFactory = ...

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<EvDbRelationalStorageAdminFactory>();
            IEvDbStorageAdmin adapter = PostgresStorageAdminFactory.Create(logger, connectionString, ctx, shardNames);
            return adapter;
        });

        return services;
    }
}
