// Ignore Spelling: Sql

using EvDb.Adapters.Store.Postgres;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for PostgreSql Storage Adapter
/// </summary>
public static class EvDbPostgresStorageAdapterDI
{
    public static void UsePostgresStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UsePostgresStoreForEvDbStream("EvDbPostgresConnection", transformers);

    public static void UsePostgresStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = "EvDbPostgresConnection",
        params IEvDbOutboxTransformer[] transformers)
        => instance.UsePostgresStoreForEvDbStream(transformers, connectionStringOrConfigurationKey);

    public static void UsePostgresStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = "EvDbPostgresConnection")
    {
        IServiceCollection services = instance.Services;
        EvDbPartitionAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedSingleton(
            key.ToString(),
            (sp, _) =>
                {
                    var ctx = context
                        ?? sp.GetService<EvDbStorageContext>()
                        ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                    #region IEvDbConnectionFactory connectionFactory = ...

                    string connectionString;
                    IConfiguration? configuration = sp.GetService<IConfiguration>();
                    connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                    #endregion // IEvDbConnectionFactory connectionFactory = ...

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbPostgresStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = EvDbPostgresStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, ctx, transformers);
                    return adapter;
                });
    }

    public static void UsePostgresForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = "EvDbPostgresConnection")
    {
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedSingleton<IEvDbStorageSnapshotAdapter>(
            key.ToString(),
            (sp, _) =>
                {
                    var ctx = context
                        ?? sp.GetService<EvDbStorageContext>()
                        ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                    #region IEvDbConnectionFactory connectionFactory = ...

                    IConfiguration? configuration = sp.GetService<IConfiguration>();
                    string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                    #endregion // IEvDbConnectionFactory connectionFactory = ...

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbPostgresStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = EvDbPostgresStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                    return adapter;
                });
    }
}
