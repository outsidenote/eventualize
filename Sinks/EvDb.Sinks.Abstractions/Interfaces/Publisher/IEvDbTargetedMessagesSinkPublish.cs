using EvDb.Core;

namespace EvDb.Sinks;

/// <summary>
/// Specialized version that use a specific Sink provider and publish to a predefined target (topic/queue)
/// </summary>
public interface IEvDbTargetedMessagesSinkPublish
{
    /// <summary>
    /// Publishes a message to the specified sink and specific target (topic/queue).
    /// </summary>
    /// <param name="message">the payload</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishMessageToSinkAsync(EvDbMessage message, CancellationToken cancellationToken = default);
}