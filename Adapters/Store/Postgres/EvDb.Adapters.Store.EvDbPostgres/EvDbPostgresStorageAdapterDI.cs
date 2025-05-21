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
    private const string DEFAULT_CONNECTION_STRING_KEY = "EvDbPostgresConnection";

    #region UsePostgresStoreForEvDbStream

    public static void UsePostgresStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UsePostgresStoreForEvDbStream(DEFAULT_CONNECTION_STRING_KEY, transformers);

    public static void UsePostgresStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
        params IEvDbOutboxTransformer[] transformers)
        => instance.UsePostgresStoreForEvDbStream(transformers, connectionStringOrConfigurationKey);

    public static void UsePostgresStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        IEvDbRegistrationContext entry = instance;
        IServiceCollection services = entry.Services;
        EvDbRootAddressName key = entry.Address;
        var context = entry.Context;
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

    #endregion //  UsePostgresStoreForEvDbStream

    #region UsePostgresForEvDbSnapshot

    public static void UsePostgresForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        UsePostgresForEvDbSnapshot(instance, instance.Context, connectionStringOrConfigurationKey);
    }

    public static void UsePostgresForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            EvDbStorageContext? context,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        context = context ?? instance.Context;
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;
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

    #endregion //  UsePostgresForEvDbSnapshot

    #region UseTypedPostgresForEvDbSnapshot

    /// <summary>
    /// Uses the typed SQL server adapter for EvDb snapshot.
    /// </summary>
    /// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="filter">Filter strategy of what payload it can handle.</param>
    public static void UseTypedPostgresForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            Predicate<EvDbViewAddress> filter)
        where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        instance.UseTypedPostgresForEvDbSnapshot<T>(DEFAULT_CONNECTION_STRING_KEY, filter);
    }


    /// <summary>
    /// Uses the typed SQL server adapter for EvDb snapshot.
    /// </summary>
    /// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="connectionStringOrConfigurationKey">
    /// Connection string or configuration key of it.
    /// </param>
    /// <param name="filter">Filter strategy of what payload it can handle.</param>
    public static void UseTypedPostgresForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
            Predicate<EvDbViewAddress>? filter = null)
        where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;

        var context = instance.Context;
        string fullKey = $"{key}.for-typed-decorator.postgres";

        services.AddKeyedSingleton<IEvDbTypedSnapshotStorageAdapterFactory, T>(fullKey);

        services.AddKeyedSingleton<IEvDbStorageSnapshotAdapter>(
            fullKey,

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
                var logger = loggerFactory.CreateLogger<T>();
                IEvDbStorageSnapshotAdapter adapter = EvDbPostgresStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                return adapter;
            });

        services.AddKeyedSingleton<IEvDbTypedStorageSnapshotAdapter>(
            key.ToString(),

            (sp, _) =>
            {
                IEvDbStorageSnapshotAdapter adapter =
                            sp.GetRequiredKeyedService<IEvDbStorageSnapshotAdapter>(fullKey);
                IEvDbTypedSnapshotStorageAdapterFactory factory =
                            sp.GetRequiredKeyedService<IEvDbTypedSnapshotStorageAdapterFactory>(fullKey);
                var result = factory.Create(adapter, filter);
                return result;
            });
    }

    #endregion //  UseTypedPostgresForEvDbSnapshot
}
