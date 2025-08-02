// Ignore Spelling: Sql

using EvDb.Adapters.Internals;
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
    private const string DEFAULT_CONNECTION_STRING_KEY = "EvDbSqlServerConnection";

    #region UseSqlServerStoreForEvDbStream

    public static void UseSqlServerStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UseSqlServerStoreForEvDbStream(DEFAULT_CONNECTION_STRING_KEY, transformers);

    public static void UseSqlServerStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
        params IEvDbOutboxTransformer[] transformers)
        => instance.UseSqlServerStoreForEvDbStream(transformers, connectionStringOrConfigurationKey);

    public static void UseSqlServerStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        IEvDbRegistrationContext entry = instance;
        IServiceCollection services = entry.Services;
        EvDbStreamTypeName key = entry.Address;
        var context = entry.Context;
        services.AddKeyedSingleton(
            key.ToString(),
            (sp, _) =>
                {
                    var ctx = sp.GetEvDbStorageContextFallback(context);

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

    #region GetSqlServerChangeStream

    public static IEvDbChangeStream GetSqlServerChangeStream(
            this EvDbStorageContext context,
            ILogger logger,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
            IConfiguration? configuration = null)
    {
        string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

        IEvDbChangeStream storageAdapter = EvDbSqlServerStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []);
        return storageAdapter;
    }

    #endregion //  GetSqlServerChangeStream

    #region UseSqlServerChangeStream

    /// <summary>
    /// Use change stream of sql-server
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseSqlServerChangeStream(
            this IServiceCollection services,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddSingleton((sp) => ChangeStreamFactory(sp, null, connectionStringOrConfigurationKey));
    }

    /// <summary>
    /// Use change stream of sql-server
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseSqlServerChangeStream(
            this IServiceCollection services,
            EvDbStorageContext? context,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddSingleton((sp) => ChangeStreamFactory(sp, context, connectionStringOrConfigurationKey));
    }

    #endregion //  UseSqlServerChangeStream

    #region UseKeyedSqlServerChangeStream

    /// <summary>
    /// Use change stream of sql-server 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="services"></param>
    /// <param name="key">The Microsoft.Extensions.DependencyInjection.ServiceDescriptor.ServiceKey of the service</param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseKeyedSqlServerChangeStream<TKey>(
            this IServiceCollection services,
            TKey key,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddKeyedSingleton(key, (sp, _) => ChangeStreamFactory(sp, null, connectionStringOrConfigurationKey));
    }

    /// <summary>
    /// Use change stream of sql-server 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <param name="services"></param>
    /// <param name="key">The Microsoft.Extensions.DependencyInjection.ServiceDescriptor.ServiceKey of the service</param>
    /// <param name="context"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseKeyedSqlServerChangeStream<TKey>(
            this IServiceCollection services,
            TKey key,
            EvDbStorageContext context,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddKeyedSingleton(key, (sp, _) => ChangeStreamFactory(sp, context, connectionStringOrConfigurationKey));
    }

    #endregion //  UseKeyedSqlServerChangeStream

    #region ChangeStreamFactory

    private static IEvDbChangeStream ChangeStreamFactory(IServiceProvider sp,
                                                         EvDbStorageContext? context,
                                                         string connectionStringOrConfigurationKey)
    {
        var ctx = sp.GetEvDbStorageContextFallback(context);

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<EvDbSqlServerStorageAdapter>();
        IConfiguration? configuration = sp.GetService<IConfiguration>();

        IEvDbChangeStream storageAdapter = ctx.GetSqlServerChangeStream(logger, connectionStringOrConfigurationKey, configuration);
        return storageAdapter;
    }

    #endregion //  ChangeStreamFactory

    #region UseSqlServerForEvDbSnapshot

    public static void UseSqlServerForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        UseSqlServerForEvDbSnapshot(instance, instance.Context, connectionStringOrConfigurationKey);
    }

    public static void UseSqlServerForEvDbSnapshot(
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
                    var ctx = sp.GetEvDbStorageContextFallback(context);

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

    #region // UseTypedSqlServerForEvDbSnapshot

    ///// <summary>
    ///// Uses the typed SQL server adapter for EvDb snapshot.
    ///// </summary>
    ///// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    ///// <param name="instance">The instance.</param>
    ///// <param name="filter">Filter strategy of what payload it can handle.</param>
    //public static void UseTypedSqlServerForEvDbSnapshot<T>(
    //        this EvDbSnapshotStoreRegistrationContext instance,
    //        Predicate<EvDbViewAddress> filter)
    //    where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    //{
    //    instance.UseTypedSqlServerForEvDbSnapshot<T>(DEFAULT_CONNECTION_STRING_KEY, filter);
    //}


    ///// <summary>
    ///// Uses the typed SQL server adapter for EvDb snapshot.
    ///// </summary>
    ///// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    ///// <param name="instance">The instance.</param>
    ///// <param name="connectionStringOrConfigurationKey">
    ///// Connection string or configuration key of it.
    ///// </param>
    ///// <param name="filter">Filter strategy of what payload it can handle.</param>
    //public static void UseTypedSqlServerForEvDbSnapshot<T>(
    //        this EvDbSnapshotStoreRegistrationContext instance,
    //        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
    //        Predicate<EvDbViewAddress>? filter = null)
    //    where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    //{
    //    IServiceCollection services = instance.Services;
    //    EvDbViewBasicAddress key = instance.Address;

    //    var context = instance.Context;
    //    string fullKey = $"{key}.for-typed-decorator.sql-server";

    //    services.AddKeyedSingleton<IEvDbTypedSnapshotStorageAdapterFactory, T>(fullKey);

    //    services.AddKeyedSingleton<IEvDbStorageSnapshotAdapter>(
    //        fullKey,

    //        (sp, _) =>
    //        {
    //            var ctx = sp.GetEvDbStorageContextFallback(context);

    //            #region IEvDbConnectionFactory connectionFactory = ...

    //            IConfiguration? configuration = sp.GetService<IConfiguration>();
    //            string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

    //            #endregion // IEvDbConnectionFactory connectionFactory = ...

    //            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    //            var logger = loggerFactory.CreateLogger<T>();
    //            IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
    //            return adapter;
    //        });

    //    services.AddKeyedSingleton<IEvDbTypedStorageSnapshotAdapter>(
    //        key.ToString(),

    //        (sp, _) =>
    //        {
    //            IEvDbStorageSnapshotAdapter adapter =
    //                        sp.GetRequiredKeyedService<IEvDbStorageSnapshotAdapter>(fullKey);
    //            IEvDbTypedSnapshotStorageAdapterFactory factory =
    //                        sp.GetRequiredKeyedService<IEvDbTypedSnapshotStorageAdapterFactory>(fullKey);
    //            var result = factory.Create(adapter, filter);
    //            return result;
    //        });
    //}

    #endregion //  UseTypedSqlServerForEvDbSnapshot
}
