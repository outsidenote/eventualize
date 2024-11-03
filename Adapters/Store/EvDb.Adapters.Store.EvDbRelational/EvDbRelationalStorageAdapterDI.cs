// Ignore Spelling: Sql

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for SqlServer Storage Adapter
/// </summary>
public static class EvDbRelationalStorageAdapterDI
{
    public static IServiceCollection UseRelationalStore(
            this IServiceCollection services)
    {
        // TODO: register the metrics

        return services;
    }
}
