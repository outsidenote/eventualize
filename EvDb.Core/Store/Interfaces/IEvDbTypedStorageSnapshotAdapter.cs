namespace EvDb.Core;

public interface IEvDbTypedStorageSnapshotAdapter
{
    /// <summary>
    /// Indication whether the adapter is compatible with the state type.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="viewAddress">The view address.</param>
    /// <returns>
    /// true: the adapter is compatible with the state type.
    /// false: the adapter is not compatible with the state type.
    /// </returns>
    bool CanHandle<TState>(EvDbViewAddress viewAddress);

    /// <summary>
    /// Gets the latests stored view's snapshot or an empty snapshot if not exists.
    /// </summary>
    /// <param name="viewAddress">The view address.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    Task<EvDbStoredSnapshotBase> GetSnapshotAsync(
                                EvDbViewAddress viewAddress,
                                CancellationToken cancellation = default);

    /// <summary>
    /// Store the view's state as a snapshot
    /// </summary>
    /// <param name="data">The snapshot's snapshotData and metadata</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task StoreSnapshotAsync(
        EvDbStoredSnapshotDataBase data,
        CancellationToken cancellation = default);
}
