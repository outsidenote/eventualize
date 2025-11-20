// Ignore Spelling: Mongo

using EvDb.Adapters.Internals;
using EvDb.Adapters.Store.MongoDB;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for PostgreSql Storage Adapter
/// </summary>
public static class EvDbMongoDBStorageAdapterDI
{
    private const string DEFAULT_CONNECTION_STRING_KEY = "EvDbMongoDBConnection";

    #region UseMongoDBStoreForEvDbStream

    public static void UseMongoDBStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UseMongoDBStoreForEvDbStream(DEFAULT_CONNECTION_STRING_KEY, EvDbMongoDBCreationMode.None, transformers);

    public static void UseMongoDBStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None,
        params IEvDbOutboxTransformer[] transformers) =>
        instance.UseMongoDBStoreForEvDbStream(DEFAULT_CONNECTION_STRING_KEY, creationMode, transformers);

    public static void UseMongoDBStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
        params IEvDbOutboxTransformer[] transformers)
        => instance.UseMongoDBStoreForEvDbStream(transformers, connectionStringOrConfigurationKey, EvDbMongoDBCreationMode.None);

    public static void UseMongoDBStoreForEvDbStream(
        this EvDbStreamStoreRegistrationContext instance,
        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
        EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None,
        params IEvDbOutboxTransformer[] transformers)
        => instance.UseMongoDBStoreForEvDbStream(transformers, connectionStringOrConfigurationKey, creationMode);

    public static void UseMongoDBStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
            EvDbMongoDBCreationMode creationMode = EvDbMongoDBCreationMode.None)
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
                    var logger = loggerFactory.CreateLogger<EvDbMongoDBStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = EvDbMongoDBStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, ctx, transformers, creationMode);
                    return adapter;
                });
    }

    #endregion //  UseMongoDBStoreForEvDbStream

    #region GetMongoDBChangeStream

    public static IEvDbChangeStream GetMongoDBChangeStream(
            this EvDbStorageContext context,
            ILogger logger,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
            IConfiguration? configuration = null)
    {
        string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

        IEvDbChangeStream storageAdapter = EvDbMongoDBStorageAdapterFactory.CreateStreamAdapter(logger, connectionString, context, []);
        return storageAdapter;
    }

    #endregion //  GetMongoDBChangeStream

    #region UseMongoDBChangeStream

    /// <summary>
    /// Register change stream
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseMongoDBChangeStream(
            this IServiceCollection services,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddSingleton((sp) => ChangeStreamFactory(sp, null, connectionStringOrConfigurationKey));
    }

    /// <summary>
    /// Register change stream
    /// </summary>
    /// <param name="services"></param>
    /// <param name="context"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseMongoDBChangeStream(
            this IServiceCollection services,
            EvDbStorageContext? context,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddSingleton((sp) => ChangeStreamFactory(sp, context, connectionStringOrConfigurationKey));
    }

    #endregion //  UseMongoDBChangeStream

    #region UseKeyedMongoDBChangeStream

    /// <summary>
    /// Register change stream with a specific key 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">The Microsoft.Extensions.DependencyInjection.ServiceDescriptor.ServiceKey of the service</param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseKeyedMongoDBChangeStream<TKey>(
            this IServiceCollection services,
            TKey key,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddKeyedSingleton(key, (sp, _) => ChangeStreamFactory(sp, null, connectionStringOrConfigurationKey));
    }

    /// <summary>
    /// Register change stream with a specific key 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">The Microsoft.Extensions.DependencyInjection.ServiceDescriptor.ServiceKey of the service</param>
    /// <param name="context"></param>
    /// <param name="connectionStringOrConfigurationKey"></param>
    public static void UseKeyedMongoDBChangeStream<TKey>(
            this IServiceCollection services,
            TKey key,
            EvDbStorageContext? context,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        services.AddKeyedSingleton(key, (sp, _) => ChangeStreamFactory(sp, context, connectionStringOrConfigurationKey));
    }

    #endregion //  UseKeyedMongoDBChangeStream

    #region ChangeStreamFactory

    private static IEvDbChangeStream ChangeStreamFactory(IServiceProvider sp,
                                                         EvDbStorageContext? context,
                                                         string connectionStringOrConfigurationKey)
    {
        var ctx = sp.GetEvDbStorageContextFallback(context);

        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<EvDbMongoDBStorageAdapter>();
        IConfiguration? configuration = sp.GetService<IConfiguration>();

        IEvDbChangeStream storageAdapter = ctx.GetMongoDBChangeStream(logger, connectionStringOrConfigurationKey, configuration);
        return storageAdapter;
    }

    #endregion //  ChangeStreamFactory

    #region UseMongoDBForEvDbSnapshot

    public static void UseMongoDBForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY)
    {
        UseMongoDBForEvDbSnapshot(instance, instance.Context, connectionStringOrConfigurationKey);
    }

    public static void UseMongoDBForEvDbSnapshot(
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
                    var logger = loggerFactory.CreateLogger<EvDbMongoDBStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = EvDbMongoDBStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                    return adapter;
                });
    }

    #endregion //  UseMongoDBForEvDbSnapshot

    #region // UseTypedMongoDBForEvDbSnapshot

    ///// <summary>
    ///// Uses the typed SQL server adapter for EvDb snapshot.
    ///// </summary>
    ///// <typeparam name="T">The Typed snapshot adapter factory</typeparam>
    ///// <param name="instance">The instance.</param>
    ///// <param name="filter">Filter strategy of what payload it can handle.</param>
    //public static void UseTypedMongoDBForEvDbSnapshot<T>(
    //        this EvDbSnapshotStoreRegistrationContext instance,
    //        Predicate<EvDbViewAddress> filter)
    //    where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    //{
    //    instance.UseTypedMongoDBForEvDbSnapshot<T>(DEFAULT_CONNECTION_STRING_KEY, filter);
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
    //public static void UseTypedMongoDBForEvDbSnapshot<T>(
    //        this EvDbSnapshotStoreRegistrationContext instance,
    //        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
    //        Predicate<EvDbViewAddress>? filter = null)
    //    where T : class, IEvDbTypedSnapshotStorageAdapterFactory
    //{
    //    IServiceCollection services = instance.Services;
    //    EvDbViewBasicAddress key = instance.Address;

    //    var context = instance.Context;
    //    string fullKey = $"{key}.for-typed-decorator.postgres";

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
    //            IEvDbStorageSnapshotAdapter adapter = EvDbMongoDBStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
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

    #endregion //  UseTypedMongoDBForEvDbSnapshot
}
