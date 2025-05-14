// Ignore Spelling: Sql Testing

using EvDb.Adapters.Store.Testing;
using EvDb.Core;
using EvDb.Core.Store.Internals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for PostgreSql Storage Adapter
/// </summary>
public static class EvDbTestingStorageAdapterDI
{
    #region UseTestingStoreForEvDbStream

    public static void UseTestingStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            IEnumerable<IEvDbOutboxTransformer> transformers,
            EvDbStreamTestingStorage? storage = null)
    {
        IEvDbRegistrationContext entry = instance;
        IServiceCollection services = entry.Services;
        EvDbPartitionAddress key = entry.Address;
        var context = entry.Context;
        services.AddKeyedSingleton(
            key.ToString(),
            (sp, _) =>
                {
                    storage = storage
                        ?? sp.GetService<EvDbStreamTestingStorage>()
                        ?? new EvDbStreamTestingStorage();
                    var ctx = context
                        ?? sp.GetService<EvDbStorageContext>()
                        ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbTestingStorageAdapter>();
                    IEvDbStorageStreamAdapter adapter = new EvDbTestingStorageAdapter(
                                                                                logger,
                                                                                ctx,
                                                                                transformers,
                                                                                storage);
                    return adapter;
                });
    }

    #endregion //  UseTestingStoreForEvDbStream

    #region UseTestingForEvDbSnapshot


    public static void UseTestingForEvDbSnapshot(
            this EvDbSnapshotStoreRegistrationContext instance,
            EvDbStorageContext? context,
            EvDbStreamTestingStorage? storage = null)
    {
        context = context ?? instance.Context;
        IServiceCollection services = instance.Services;
        EvDbViewBasicAddress key = instance.Address;
        services.AddKeyedSingleton<IEvDbStorageSnapshotAdapter>(
            key.ToString(),
            (sp, _) =>
                {
                    storage = storage
                       ?? sp.GetService<EvDbStreamTestingStorage>()
                       ?? new EvDbStreamTestingStorage();
                    var ctx = context
                        ?? sp.GetService<EvDbStorageContext>()
                        ?? EvDbStorageContext.CreateWithEnvironment("evdb");

                    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<EvDbTestingStorageAdapter>();
                    IEvDbStorageSnapshotAdapter adapter = new EvDbTestingStorageAdapter(
                                                                                logger,
                                                                                ctx,
                                                                                [],
                                                                                storage);
                    return adapter;
                });
    }

    #endregion //  UseTestingForEvDbSnapshot
}
