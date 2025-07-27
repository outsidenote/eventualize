using EvDb.Core;
using System.Text.Json;

namespace EvDb.Sinks;

/// <summary>
/// Specialized version that use a specific Sink provider and publish to a predefined target (topic/queue)
/// </summary>
public interface IEvDbTargetedMessagesSinkPublish
{
    /// <summary>
    /// The kind of the sink, e.g. "SNS", "SQS", "Kafka", etc.
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Publishes a message to the specified sink and specific target (topic/queue).
    /// </summary>
    /// <param name="message">the payload</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishMessageToSinkAsync(EvDbMessage message, CancellationToken cancellationToken = default) =>
                                    PublishMessageToSinkAsync(message, null, cancellationToken);

    /// <summary>
    /// Publishes a message to the specified sink and specific target (topic/queue).
    /// </summary>
    /// <param name="message">the payload</param>
    /// <param name="serializerOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishMessageToSinkAsync(EvDbMessage message,
                                   JsonSerializerOptions? serializerOptions,
                                   CancellationToken cancellationToken = default);
}