namespace EvDb.Core;

public interface IEvDbStorageSnapshotAdapter
{
    /// <summary>
    /// Gets the latests stored view's snapshot or an empty snapshot if not exists.
    /// </summary>
    /// <param name="viewAddress">The view address.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    Task<EvDbStoredSnapshot> GetSnapshotAsync(
                                EvDbViewAddress viewAddress,
                                CancellationToken cancellation = default);

    /// <summary>
    /// Store the view's state as a snapshot
    /// </summary>
    /// <param name="snapshotData">The snapshot's snapshotData and metadata</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task StoreSnapshotAsync(
        EvDbStoredSnapshotData snapshotData,
        CancellationToken cancellation = default);
}
