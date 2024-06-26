﻿// Ignore Spelling: Sql

using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbSqlServerStorageMigrationDI
{
    public static IServiceCollection AddEvDbSqlServerStoreMigrationFromStringOrEnvironmentKey(
            this IServiceCollection services,
            string connectionStringOrKey)
    {
        return services.AddEvDbSqlServerStoreMigrationFromStringOrEnvironmentKey(connectionStringOrKey: connectionStringOrKey);
    }

    public static IServiceCollection AddEvDbSqlServerStoreMigration(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbSqlServerConnection")
    {
        services.AddScoped(sp =>
        {
            var ctx = context
                ?? sp.GetService<EvDbStorageContext>()
                ?? EvDbStorageContext.CreateWithEnvironment("evdb");

            #region IEvDbConnectionFactory connectionFactory = ...

            string connectionString;
            IConfiguration? configuration = sp.GetService<IConfiguration>();
            connectionString = configuration?.GetConnectionString(connectionStringOrKey) ?? connectionStringOrKey;
            IEvDbConnectionFactory connectionFactory = new EvDbSqlConnectionFactory(connectionString);

            #endregion // IEvDbConnectionFactory connectionFactory = ...

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<EvDbRelationalStorageMigration>();
            IEvDbStorageMigration adapter = SqlServerStorageMigrationFactory.Create(logger, connectionString, ctx);
            return adapter;
        });

        return services;
    }
}
