using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamStoreData
{
    /// <summary>
    /// Serialization options
    /// </summary>
    JsonSerializerOptions? Options { get; }

    /// <summary>
    /// Views (unspecialized)
    /// </summary>
    IEnumerable<IEvDbView> Views { get; }

    /// <summary>
    /// Unspecialized events
    /// </summary>
    IEnumerable<EvDbEvent> Events { get; }

    /// <summary>
    /// Unspecialized notifications
    /// </summary>
    IEnumerable<EvDbMessage> Notifications { get; }

    /// <summary>
    /// Gets the stream address (uniqueness).
    /// </summary>
    EvDbStreamAddress StreamAddress { get; }
}
