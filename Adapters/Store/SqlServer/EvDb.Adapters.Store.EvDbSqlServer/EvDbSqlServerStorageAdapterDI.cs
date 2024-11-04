// Ignore Spelling: Sql

using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class EvDbSqlServerStorageAdapterDI
{
    public static void UseSqlServerStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UseSqlServerStoreForEvDbStream("EvDbSqlServerConnection", transformers);

    public static void UseSqlServerStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = "EvDbSqlServerConnection",
        params IEvDbOutboxTransformer[] transformers)
        => instance.UseSqlServerStoreForEvDbStream(transformers, connectionStringOrConfigurationKey);

    public static void UseSqlServerStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
    {
        IServiceCollection services = instance.Services;
        services.UseRelationalStore();
        EvDbPartitionAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedScoped(
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
                    var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, ctx, transformers);
                    return adapter;
                });
    }

    public static void UseSqlServerForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
    {
        IServiceCollection services = instance.Services;
        services.UseRelationalStore();
        EvDbViewBasicAddress key = instance.Address;
        var context = instance.Context;
        services.AddKeyedScoped<IEvDbStorageSnapshotAdapter>(
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
                    var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                    return adapter;
                });
    }
}
