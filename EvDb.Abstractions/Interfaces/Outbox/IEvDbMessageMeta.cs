using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Messsage metadata
/// </summary>
public interface IEvDbMessageMeta : IEvDbEventMeta
{
    /// <summary>
    /// The message type
    /// </summary>
    string MessageType { get; }

    /// <summary>
    /// A channel attached to the message
    /// </summary>
    EvDbChannelName Channel { get; }
}
