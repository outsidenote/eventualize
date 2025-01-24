// Ignore Spelling: Sql

using EvDb.Adapters.Store.SqlServer;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using EvDb.IntegrationTests.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class PersonSnapshotStorageAdapterDI
{
    public static void UsePersonSqlServerForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            Func<TypedStorageOptions, TypedStorageOptions>? options = null)
    {
        TypedStorageOptions setting = options?.Invoke(TypedStorageOptions.Default) ?? TypedStorageOptions.Default;
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;

        services.AddSqlDbContextFactory<PersonContext>(setting);

        var context = instance.Context;
        services.AddKeyedSingleton<IEvDbTypedStorageSnapshotAdapter>(
            key.ToString(),

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
                var logger = loggerFactory.CreateLogger<EvDbPersonStorageStreamAdapter>();
                IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                var personContext = sp.GetRequiredService<IDbContextFactory<PersonContext>>();
                var typedAdapter = new EvDbPersonStorageStreamAdapter(personContext, adapter);
                return typedAdapter;
            });
    }


    public static IServiceCollection AddSqlDbContextFactory<TContext>(
                                        this IServiceCollection services,
                                        TypedStorageOptions setting,
                                        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>(
                (sp, optionsBuilder) =>
                {
                    IConfiguration? configuration = sp.GetService<IConfiguration>();
                    string connectionStringOrConfigurationKey = setting.ContextConnectionStringOrConfigurationKey;
                    string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                    optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.CommandTimeout(setting.CommandTimeout);
                        //sqlServerOptions.EnableRetryOnFailure(setting., TimeSpan.FromSeconds(dbResilienceSettings.MaxRetryDelaySeconds), null);
                    });
                }, lifetime);

        return services;
    }

    public static void UseTypedSqlServerForEvDbSnapshot<T>(
            this EvDbSnapshotStoreRegistrationContext instance,
            Func<TypedStorageOptions, TypedStorageOptions>? options = null,
            Predicate<EvDbViewAddress>? canHandle = null)
        where T: class, IEvDbTypedSnapshotStorageAdapterFactory
    {
        TypedStorageOptions setting = options?.Invoke(TypedStorageOptions.Default) ?? TypedStorageOptions.Default;
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
                var logger = loggerFactory.CreateLogger<EvDbPersonStorageStreamAdapter>();
                IEvDbStorageSnapshotAdapter adapter = EvDbSqlServerStorageAdapterFactory.CreateSnapshotAdapter(logger, connectionString, ctx);
                return adapter;
            });

        services.AddKeyedSingleton<IEvDbTypedStorageSnapshotAdapter>(
            key.ToString(),

            (sp, _) =>
            {
                IEvDbStorageSnapshotAdapter adapter = 
                            sp.GetKeyedService<IEvDbStorageSnapshotAdapter>(fullKey) ?? throw new MissingMemberException($"`{nameof(IEvDbStorageSnapshotAdapter)}` is missing, expected to be registered under `{fullKey}` key.");
                IEvDbTypedSnapshotStorageAdapterFactory factory = 
                            sp.GetKeyedService<IEvDbTypedSnapshotStorageAdapterFactory>(fullKey) ?? throw new MissingMemberException($"`{nameof(IEvDbStorageSnapshotAdapter)}` is missing, expected to be registered under `{fullKey}` key.");
                var result = factory.Create(adapter, canHandle);
                return result;
            });
    }
}
