// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.Core.Store;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
