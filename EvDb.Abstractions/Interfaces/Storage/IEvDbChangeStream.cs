using EvDb.Core.Adapters;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace EvDb.Core;

public interface IEvDbChangeStream
{
    /// <summary>
    /// Gets stream of stored messages.
    /// </summary>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>Stream of messages</returns>
    IAsyncEnumerable<EvDbMessage> GetFromOutboxAsync(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.GetFromOutboxAsync(EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Gets stream of stored messages.
    /// </summary>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>Stream of messages</returns>
    async IAsyncEnumerable<EvDbMessage> GetFromOutboxAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (EvDbMessageRecord record in this.GetRecordsFromOutboxAsync(shard, filter, options, cancellation))
        {
            EvDbMessage message = record;
            yield return message;
        }
    }

    /// <summary>
    /// Gets stored messages.
    /// </summary>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>Stream of messages</returns>
    IAsyncEnumerable<EvDbMessageRecord> GetRecordsFromOutboxAsync(
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.GetRecordsFromOutboxAsync(EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Gets stream of stored messages.
    /// </summary>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns>Stream of messages</returns>
    IAsyncEnumerable<EvDbMessageRecord> GetRecordsFromOutboxAsync(
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default);

    /// <summary>
    /// Subscribe to a stream of stored messages into via Dataflow Block.
    /// You can control the concurrency and back pressure of the Dataflow Block to control how many messages will be processed in parallel and BoundedCapacity.
    /// Complete the Dataflow Block when the stream is completed or cancelled.
    /// </summary>
    /// <param name="handler">The subscription handler</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SubscribeToMessageAsync(
                                ITargetBlock<EvDbMessage> handler,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.SubscribeToMessageAsync(handler, EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Subscribe to a stream of stored messages into via Dataflow Block.
    /// You can control the concurrency and back pressure of the Dataflow Block to control how many messages will be processed in parallel and BoundedCapacity.
    /// Complete the Dataflow Block when the stream is completed or cancelled.
    /// </summary>
    /// <param name="handler">The subscription handler</param>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    async Task SubscribeToMessageAsync(
                                ITargetBlock<EvDbMessage> handler,
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default)
    {
        IEvDbChangeStream self = this;
        IAsyncEnumerable<EvDbMessage> stream = self.GetFromOutboxAsync(shard, filter, options, cancellation);
        await foreach (EvDbMessage m in stream)
        {
            #region Validation

            if (handler.Completion.IsCompleted)
                break; // if the Dataflow Block is completed, stop processing

            if (cancellation.IsCancellationRequested)
                break; // if the cancellation is requested, stop processing

            #endregion //  Validation

            if (!await handler.SendAsync(m, cancellation).FalseWhenCancelAsync())
                break;
        }
        handler.Complete(); // indicate that the stream is completed, no more messages will be sent to the Dataflow Block
    }

    /// <summary>
    /// Subscribe to a stream of stored messages into via Dataflow Block.
    /// You can control the concurrency and back pressure of the Dataflow Block to control how many messages will be processed in parallel and BoundedCapacity.
    /// Complete the Dataflow Block when the stream is completed or cancelled.
    /// </summary>
    /// <param name="handler">The subscription handler</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SubscribeToMessageRecordsAsync(
                                ITargetBlock<EvDbMessageRecord> handler,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default) => this.SubscribeToMessageRecordsAsync(handler, EvDbShardName.Default, filter, options, cancellation);
    /// <summary>
    /// Subscribe to a stream of stored messages into via Dataflow Block.
    /// You can control the concurrency and back pressure of the Dataflow Block to control how many messages will be processed in parallel and BoundedCapacity.
    /// Complete the Dataflow Block when the stream is completed or cancelled.
    /// </summary>
    /// <param name="handler">The subscription handler</param>
    /// <param name="shard">The shard (table/collection) of the messages</param>
    /// <param name="filter">filtering options use `EvDbMessageFilter.Builder` for the filter creation.</param>
    /// <param name="options">Options for the continuous fetch.</param>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    async Task SubscribeToMessageRecordsAsync(
                                ITargetBlock<EvDbMessageRecord> handler,
                                EvDbShardName shard,
                                EvDbMessageFilter filter,
                                EvDbContinuousFetchOptions? options = null,
                                CancellationToken cancellation = default)
    {
        IEvDbChangeStream self = this;
        IAsyncEnumerable<EvDbMessageRecord> stream = self.GetRecordsFromOutboxAsync(shard, filter, options, cancellation);
        await foreach (EvDbMessageRecord m in stream)
        {
            #region Validation

            if (handler.Completion.IsCompleted)
                break; // if the Dataflow Block is completed, stop processing

            if (cancellation.IsCancellationRequested)
                break; // if the cancellation is requested, stop processing

            #endregion //  Validation

            if (!await handler.SendAsync(m, cancellation).FalseWhenCancelAsync())
                break;
        }
        handler.Complete(); // indicate that the stream is completed, no more messages will be sent to the Dataflow Block
    }
}
