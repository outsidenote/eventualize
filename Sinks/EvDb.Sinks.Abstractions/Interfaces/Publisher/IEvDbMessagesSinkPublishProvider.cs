namespace EvDb.Sinks;

/// <summary>
/// Sink provider abstraction
/// Implemented by each provider
/// </summary>
public interface IEvDbMessagesSinkPublishProvider
{
    /// <summary>
    /// Gets specialized version that use a specific Sink provider and publish to a predefined target (topic/queue)
    /// </summary>
    /// <param name="target">The destination channel (topic/queue)</param>
    /// <returns></returns>
    IEvDbTargetedMessagesSinkPublish Create(EvDbSinkTarget target);
}
