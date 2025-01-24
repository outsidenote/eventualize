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
    #region UseSqlServerStoreForEvDbStream

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
        IEvDbRegistrationContext entry = instance;
        IServiceCollection services = entry.Services;
        EvDbPartitionAddress key = entry.Address;
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
                    var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, ctx, transformers);
                    return adapter;
                });
    }

    #endregion //  UseSqlServerStoreForEvDbStream

    #region UseSqlServerForEvDbSnapshot

    public static void UseSqlServerForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
    {
        UseSqlServerForEvDbSnapshot(instance, instance.Context, connectionStringOrConfigurationKey);
    }

    public static void UseSqlServerForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            EvDbStorageContext? context,
            string connectionStringOrConfigurationKey = "EvDbSqlServerConnection")
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
                    var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                    return adapter;
                });
    }

    #endregion //  UseSqlServerForEvDbSnapshot

    #region UseTypedSqlServerForEvDbSnapshot

    /// <summary>
    /// Uses the typed SQL server adapter for EvDb snapshot.
    /// </summary>
    /// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="options">The options.</param>
    public static void UseTypedSqlServerForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            Func<TypedStorageOptions, TypedStorageOptions> options)
        where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        TypedStorageOptions setting = options.Invoke(TypedStorageOptions.Default);
        instance.UseTypedSqlServerForEvDbSnapshot<T>(setting, null);
    }

    /// <summary>
    /// Uses the typed SQL server adapter for EvDb snapshot.
    /// </summary>
    /// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="filter">Filter strategy of what payload it can handle.</param>
    /// <param name="options">The options.</param>
    public static void UseTypedSqlServerForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            Predicate<EvDbViewAddress>? filter = null,
            Func<TypedStorageOptions, TypedStorageOptions>? options = null)
        where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        TypedStorageOptions setting = options?.Invoke(TypedStorageOptions.Default) ?? TypedStorageOptions.Default;
        instance.UseTypedSqlServerForEvDbSnapshot<T>(setting, filter);
    }


    /// <summary>
    /// Uses the typed SQL server adapter for EvDb snapshot.
    /// </summary>
    /// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    /// <param name="instance">The instance.</param>
    /// <param name="setting">The setting.</param>
    /// <param name="filter">Filter strategy of what payload it can handle.</param>
    private static void UseTypedSqlServerForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            TypedStorageOptions setting,
            Predicate<EvDbViewAddress>? filter)
        where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;

        var context = instance.Context;
        string fullKey = $"{key}.for-typed-decorator";

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
                string connectionStringOrConfigurationKey = setting.EvDbConnectionStringOrConfigurationKey;
                string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                #endregion // IEvDbConnectionFactory connectionFactory = ...

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<T>();
                IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
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

    #endregion //  UseTypedSqlServerForEvDbSnapshot
}
