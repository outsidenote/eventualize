// Ignore Spelling: Sql InMemory

using EvDb.Adapters.Store.InMemory;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for PostgreSql Storage Adapter
/// </summary>
public static class EvDbInMemoryStorageAdapterDI
{
    #region UseInMemoryStoreForEvDbStream

    public static void UseInMemoryStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers)
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

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbInMemoryStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = new EvDbInMemoryStorageAdapter(
                                                                                logger,
                                                                                context,
                                                                                transformers);
                    return adapter;
                });
    }

    #endregion //  UseInMemoryStoreForEvDbStream

    #region UseInMemoryForEvDbSnapshot


    public static void UseInMemoryForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            EvDbStorageContext? context)
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

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbInMemoryStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = new EvDbInMemoryStorageAdapter(
                                                                                logger,
                                                                                context,
                                                                                []);
                    return adapter;
                });
    }

    #endregion //  UseInMemoryForEvDbSnapshot
}
