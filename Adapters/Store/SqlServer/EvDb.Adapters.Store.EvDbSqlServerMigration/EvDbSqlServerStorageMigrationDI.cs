// Ignore Spelling: Sql Admin

using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbSqlServerStorageMigrationDI
{
    #region Overloads

    public static IServiceCollection AddEvDbSqlServerStoreAdmin(
            this IServiceCollection services,
            string connectionStringOrKey,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbSqlServerStoreAdmin(
                            null,
                            connectionStringOrKey,
                            shardNames);
    }

    public static IServiceCollection AddEvDbSqlServerStoreAdmin(
            this IServiceCollection services,
            params EvDbShardName[] shardNames)
    {
        return services.AddEvDbSqlServerStoreAdmin(
                            null,
                            "EvDbSqlServerConnection",
                            shardNames);
    }

    #endregion //  Overloads

    public static IServiceCollection AddEvDbSqlServerStoreAdmin(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbSqlServerConnection",
            params EvDbShardName[] shardNames)
    {
        services.AddEvDbRelationalStoreAdmin();

        services.AddSingleton(sp =>
        {
            var ctx = context
                ?? sp.GetService<EvDbStorageContext>()
                ?? EvDbStorageContext.CreateWithEnvironment("evdb");

            ILogger logger = sp.GetRequiredService<ILogger<EvDbRelationalStorageAdminFactory>>();
            string connectionString;
            IConfiguration? configuration = sp.GetService<IConfiguration>();
            connectionString = configuration?.GetConnectionString(connectionStringOrKey) ?? connectionStringOrKey;

            IEvDbStorageAdmin adapter = SqlServerStorageAdminFactory.Create(logger, connectionString, ctx, shardNames);
            return adapter;
        });

        return services;
    }

}
