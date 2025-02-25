// Ignore Spelling: Admin

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbRelationalStorageAdminDI
{
    public static IServiceCollection AddEvDbRelationalStoreAdmin(
            this IServiceCollection services)
    {
        services.AddSingleton<IEvDbStorageAdminFactory, EvDbRelationalStorageAdminFactory>();

        return services;
    }
}
