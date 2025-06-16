using EvDb.Core;

namespace EvDb.Sinks;

/// <summary>
/// Sink provider abstraction
/// Implemented by each provider
/// </summary>
public interface IEvDbMessagesSinkPublishProvider
{
    /// <summary>
    /// Publishes a message to the specified sink.
    /// </summary>
    /// <param name="target">The destination channel (topic/queue)</param>
    /// <param name="message">the payload</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishMessageToSinkAsync(EvDbSinkTarget target, EvDbMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets specialized version that use a specific Sink provider and publish to a predefined target (topic/queue)
    /// </summary>
    /// <param name="target">The destination channel (topic/queue)</param>
    /// <returns></returns>
    IEvDbTargetedMessagesSinkPublish Create(EvDbSinkTarget target);
}
