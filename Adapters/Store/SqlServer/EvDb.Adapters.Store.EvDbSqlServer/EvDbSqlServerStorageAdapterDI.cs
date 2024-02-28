using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbSqlServerStorageAdapterDI
{
    public static IServiceCollection AddEvDbSqlServerStore(
            this IServiceCollection services,
            string connectionStringOrKey)
    {
        return services.AddEvDbSqlServerStore(null, connectionStringOrKey: connectionStringOrKey);
    }

    public static IServiceCollection AddEvDbSqlServerStore(
            this IServiceCollection services,
            EvDbStorageContext? context = null,
            string connectionStringOrKey = "EvDbSqlServerConnection")
    {
        // TODO: [bnaya 2024-02-13] Keyed injection
        services.AddSingleton<IEvDbStorageAdapter>(sp =>
        {
            #region IEvDbConnectionFactory connectionFactory = ...

            string connectionString;
            IConfiguration? configuration = sp.GetService<IConfiguration>();
            connectionString = configuration?.GetConnectionString(connectionStringOrKey) ?? connectionStringOrKey;
            IEvDbConnectionFactory connectionFactory = new EvDbSqlConnectionFactory(connectionString);

            #endregion // IEvDbConnectionFactory connectionFactory = ...

            context = context ?? EvDbStorageContext.CreateWithEnvironment("scheduling");

            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
            IEvDbStorageAdapter adapter = EvDbSqlServerStorageAdapterFactory.Create(logger, connectionString, context);
            return adapter;
        });

        return services;
    }
}
