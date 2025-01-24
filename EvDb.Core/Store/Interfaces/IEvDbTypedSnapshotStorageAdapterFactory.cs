namespace EvDb.Core;

/// <summary>
/// Factory to expose a typed snapshot storage adapter
/// </summary>
public interface IEvDbTypedSnapshotStorageAdapterFactory
{
    /// <summary>
    /// Creates the specified adapter.
    /// </summary>
    /// <param name="adapter">The adapter.</param>
    /// <param name="canHandle">The can handle filter.</param>
    /// <returns></returns>
    IEvDbTypedStorageSnapshotAdapter Create(
        IEvDbStorageSnapshotAdapter adapter,
        Predicate<EvDbViewAddress>? canHandle = null);
}
