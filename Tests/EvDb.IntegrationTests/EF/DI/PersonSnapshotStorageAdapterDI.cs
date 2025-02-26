// Ignore Spelling: Sql

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class PersonSnapshotStorageAdapterDI
{
    private const string DEFAULT_CONNECTION_STRING_KEY = "EvDbSqlServerConnection";

    #region AddSqlServerDbContextFactory

    public static IServiceCollection AddSqlServerDbContextFactory<TContext>(
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
                        //npgsqlOptions.EnableRetryOnFailure(setting., TimeSpan.FromSeconds(dbResilienceSettings.MaxRetryDelaySeconds), null);
                    });
                }, lifetime);

        return services;
    }

    #endregion //  AddSqlServerDbContextFactory

    #region AddPostgresDbContextFactory

    public static IServiceCollection AddPostgresDbContextFactory<TContext>(
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

                    optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        const int timeoutSec = 10;
                        npgsqlOptions.CommandTimeout(timeoutSec);
                        //npgsqlOptions.EnableRetryOnFailure(setting., TimeSpan.FromSeconds(dbResilienceSettings.MaxRetryDelaySeconds), null);
                    });
                }, lifetime);

        return services;
    }

    #endregion //  AddPostgresDbContextFactory
}
