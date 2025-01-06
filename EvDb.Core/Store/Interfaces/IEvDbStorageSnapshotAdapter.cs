namespace EvDb.Core;

public interface IEvDbStorageSnapshotAdapter<TState>: IEvDbStorageSnapshotAdapterBase
{
    /// <summary>
    /// Store the view's state as a snapshot
    /// </summary>
    /// <param name="data">The snapshot's snapshotData and metadata</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task StoreViewAsync(
        EvDbStoredSnapshotData<TState> data,
        CancellationToken cancellation = default);
}

public interface IEvDbStorageSnapshotAdapter: IEvDbStorageSnapshotAdapterBase
{
    /// <summary>
    /// Store the view's state as a snapshot
    /// </summary>
    /// <param name="snapshotData">The snapshot's snapshotData and metadata</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task StoreViewAsync(
        EvDbStoredSnapshotData snapshotData,
        CancellationToken cancellation = default);
}

public interface IEvDbStorageSnapshotAdapterBase
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

}