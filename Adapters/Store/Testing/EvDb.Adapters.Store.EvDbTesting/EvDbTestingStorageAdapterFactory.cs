// Ignore Spelling: Sql Testing

using EvDb.Adapters.Store.Testing;
using EvDb.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbTestingStorageAdapterFactory
{
    #region CreateStreamAdapter

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
            this EvDbStorageContext context,
            EvDbStreamTestingStorage? storage = null)
    {
#pragma warning disable S3878 // Arrays should not be created for params parameters
        return context.CreateStreamAdapter(storage, []);
#pragma warning restore S3878 // Arrays should not be created for params parameters
    }

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
            this EvDbStorageContext context,
            params IEvDbOutboxTransformer[] transformers)
    {
        return context.CreateStreamAdapter(null, transformers);
    }

    public static IEvDbStorageStreamAdapter CreateStreamAdapter(
            this EvDbStorageContext context,
            EvDbStreamTestingStorage? storage,
            params IEvDbOutboxTransformer[] transformers)
    {
        IEvDbStorageStreamAdapter adapter = new EvDbTestingStorageAdapter(
                                                                    context,
                                                                    transformers,
                                                                    storage);
        return adapter;
    }

    #endregion //  CreateStreamAdapter

    #region UseTestingForEvDbSnapshot

    public static IEvDbStorageSnapshotAdapter CreateSnapshotAdapter(
            this EvDbStorageContext context,
            EvDbStreamTestingStorage? storage = null)
    {
        storage = storage ?? new EvDbStreamTestingStorage();

        IEvDbStorageSnapshotAdapter adapter = new EvDbTestingStorageAdapter(
                                                                    context,
                                                                    [],
                                                                    storage);
        return adapter;
    }

    #endregion //  UseTestingForEvDbSnapshot
}
