// Ignore Spelling: Sql Testing

using EvDb.Core;
using EvDb.Core.Store.Internals;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extension methods for PostgreSql Storage Adapter
/// </summary>
public static class EvDbTestingStorageAdapterDI
{
    #region UseTestingStoreForEvDbStream

    public static void UseTestingStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            EvDbStreamTestingStorage? storage = null)
    {
#pragma warning disable S3878 // Arrays should not be created for params parameters
        instance.UseTestingStoreForEvDbStream(storage, []);
#pragma warning restore S3878 // Arrays should not be created for params parameters
    }

    public static void UseTestingStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            params IEvDbOutboxTransformer[] transformers)
    {
        instance.UseTestingStoreForEvDbStream(null, transformers);
    }

    public static void UseTestingStoreForEvDbStream(
            this EvDbStreamStoreRegistrationContext instance,
            EvDbStreamTestingStorage? storage,
            params IEvDbOutboxTransformer[] transformers)
    {
        IEvDbRegistrationContext entry = instance;
        IServiceCollection services = entry.Services;
        EvDbStreamTypeName key = entry.Address;
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

                    IEvDbStorageStreamAdapter adapter = ctx.CreateStreamAdapter(storage,
                                                                                transformers);
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

                    IEvDbStorageSnapshotAdapter adapter = ctx.CreateSnapshotAdapter(storage);
                    return adapter;
                });
    }

    #endregion //  UseTestingForEvDbSnapshot
}
