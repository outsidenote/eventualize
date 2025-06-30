namespace EvDb.Core;

/// <summary>
/// Message metadata
/// </summary>
public interface IEvDbMessageMeta : IEvDbEventMeta
{
    /// <summary>
    /// The message type
    /// </summary>
    EvDbMessageTypeName MessageType { get; }

    /// <summary>
    /// A channel attached to the message
    /// </summary>
    EvDbChannelName Channel { get; }
}
