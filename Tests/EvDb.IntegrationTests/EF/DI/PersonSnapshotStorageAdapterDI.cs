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
        services.AddDbContextFactory<PersonContext>(
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
                    });

        var context = instance.Context;
        services.AddKeyedScoped<IEvDbTypedStorageSnapshotAdapter>(
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
}
