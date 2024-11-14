using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbStorageStreamAdapter
{
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
    /// <param name="events">The events to save</param>
    /// <param name="messages">The messages to save.</param>
    /// <param name="streamStore">The stream store.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>
    /// Count of added events
    /// </returns>
    Task<StreamStoreAffected> StoreStreamAsync(
        IImmutableList<EvDbEvent> events,
        IImmutableList<EvDbMessage> messages,
        IEvDbStreamStoreData streamStore,
        CancellationToken cancellation = default);
}
