namespace EvDb.Core;

public interface IEvDbStorageViewAdapter : IDisposable, IAsyncDisposable
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
    /// <param name="viewStore">The view store.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveViewAsync(
        IEvDbViewStore viewStore,
        CancellationToken cancellation = default);
}