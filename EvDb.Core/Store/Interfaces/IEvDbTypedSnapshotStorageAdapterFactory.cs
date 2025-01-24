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
    /// <param name="filter">Filter strategy of what payload it can handle.</param>
    /// <returns></returns>
    IEvDbTypedStorageSnapshotAdapter Create(
        IEvDbStorageSnapshotAdapter adapter,
        Predicate<EvDbViewAddress>? filter = null);
}
