namespace EvDb.Core;

public interface IEvDbChangeStream
{
    /// <summary>
    /// Gets stored events.
    /// </summary>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbMessage> GetMessagesAsync(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.GetMessagesAsync(EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Gets stored events.
    /// </summary>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbMessage> GetMessagesAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default);
}
