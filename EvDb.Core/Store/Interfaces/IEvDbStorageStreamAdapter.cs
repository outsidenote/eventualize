using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbChangeStream
{
    /// <summary>
    /// Gets stored events.
    /// </summary>
    /// <param name="shardName">The shard name</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbMessage> GetMessagesAsync(
                                EvDbShardName shardName,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default);
}
public interface IEvDbStorageStreamAdapter: IEvDbChangeStream
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
    /// Gets last stored event's offset.
    /// Used when getting a stream that has no views.
    /// In this case the last offset fetched from the events rather than views.
    /// </summary>
    /// <param name="address">The stream address</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task<long> GetLastOffsetAsync(
                                EvDbStreamAddress address,
                                CancellationToken cancellation = default);

    /// <summary>
    /// Saves the pending events to the stream
    /// </summary>
    /// <param name="events">The events to save</param>
    /// <param name="messages">The messages to save.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>
    /// Count of added events
    /// </returns>
    Task<StreamStoreAffected> StoreStreamAsync(
        IImmutableList<EvDbEvent> events,
        IImmutableList<EvDbMessage> messages,
        CancellationToken cancellation = default);
}
