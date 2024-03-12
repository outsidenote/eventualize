namespace EvDb.Core;

public interface IEvDbStorageAdapter : IDisposable, IAsyncDisposable
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
    /// Gets stored events.
    /// </summary>
    /// <param name="streamCursor">The streamCursor.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbEvent> GetEventsAsync(
                                EvDbStreamCursor streamCursor,
                                CancellationToken cancellation = default);

    /// <summary>
    /// Saves the pending events to the stream
    /// </summary>
    /// <param name="streamStore">The stream store.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveStreamAsync(
        IEvDbStreamStoreData streamStore,
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