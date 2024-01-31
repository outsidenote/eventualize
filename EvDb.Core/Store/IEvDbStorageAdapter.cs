using System.Text.Json;

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
    IAsyncEnumerable<IEvDbStoredEvent> GetEventsAsync(
                                EvDbStreamCursor streamCursor, 
                                CancellationToken cancellation = default);

    /// <summary>
    /// Saves the pending events and create required snapshots.
    /// </summary>
    /// <param name="streamStore">The stream store.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(
        IEvDbStreamStoreData streamStore, 
        CancellationToken cancellation = default);
}