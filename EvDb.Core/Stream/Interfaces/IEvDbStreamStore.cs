namespace EvDb.Core;

public interface IEvDbStreamStore : IDisposable
{
    /// <summary>
    /// The offset of the last event that was stored.
    /// </summary>
    long StoredOffset { get; }

    /// <summary>
    /// The stream's Uri
    /// </summary>
    EvDbStreamAddress StreamAddress { get; }

    /// <summary>
    /// number of events that were not stored yet.
    /// </summary>
    int CountOfPendingEvents { get; }

    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>Count of added events</returns>
    Task<int> StoreAsync(CancellationToken cancellation = default);
}
