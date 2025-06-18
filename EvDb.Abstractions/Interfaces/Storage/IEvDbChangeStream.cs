using EvDb.Core.Adapters;
using System.Runtime.CompilerServices;

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
    async IAsyncEnumerable<EvDbMessage> GetMessagesAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                [EnumeratorCancellation] CancellationToken cancellation = default)
    { 
        await foreach (EvDbMessageRecord record in this.GetMessageRecordsAsync(shard, filter, options, cancellation))
        {
            EvDbMessage message = record;
            yield return message;
        }
    }

    /// <summary>
    /// Gets stored events.
    /// </summary>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbMessageRecord> GetMessageRecordsAsync(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.GetMessageRecordsAsync(EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Gets stored events.
    /// </summary>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    IAsyncEnumerable<EvDbMessageRecord> GetMessageRecordsAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default);
}
