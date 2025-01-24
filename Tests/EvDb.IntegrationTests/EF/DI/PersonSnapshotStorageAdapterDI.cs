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
    private const string DEFAULT_CONNECTION_STRING_KEY = "EvDbSqlServerConnection";


    public static IServiceCollection AddSqlDbContextFactory<TContext>(
                                        this IServiceCollection services,
                                        string connectionStringOrConfigurationKey = DEFAULT_CONNECTION_STRING_KEY,
                                        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TContext : DbContext
    {
        services.AddDbContextFactory<TContext>(
                (sp, optionsBuilder) =>
                {
                    IConfiguration? configuration = sp.GetService<IConfiguration>();
                    string connectionString = configuration?.GetConnectionString(connectionStringOrConfigurationKey) ?? connectionStringOrConfigurationKey;

                    optionsBuilder.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        const int timeoutSec = 10;
                        sqlServerOptions.CommandTimeout(timeoutSec);
                        //sqlServerOptions.EnableRetryOnFailure(setting., TimeSpan.FromSeconds(dbResilienceSettings.MaxRetryDelaySeconds), null);
                    });
                }, lifetime);

        return services;
    }
}
