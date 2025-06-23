using EvDb.Core.Adapters;
using System.Text.Json;

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
    Task PublishMessageToSinkAsync(EvDbSinkTarget target, EvDbMessageRecord message, CancellationToken cancellationToken = default) =>
        PublishMessageToSinkAsync(target, message, null, cancellationToken);

    /// <summary>
    /// Publishes a message to the specified sink.
    /// </summary>
    /// <param name="target">The destination channel (topic/queue)</param>
    /// <param name="message">the payload</param>
    /// <param name="serializerOptions">Serializer options to use for serialization</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishMessageToSinkAsync(EvDbSinkTarget target, EvDbMessageRecord message, JsonSerializerOptions? serializerOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets specialized version that use a specific Sink provider and publish to a predefined target (topic/queue)
    /// </summary>
    /// <param name="target">The destination channel (topic/queue)</param>
    /// <returns></returns>
    IEvDbTargetedMessagesSinkPublish Create(EvDbSinkTarget target);
}
