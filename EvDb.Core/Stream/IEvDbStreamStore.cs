using System.Collections.Immutable;
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStreamStoreData
{
    /// <summary>
    /// Bookmark to the last successful event persistence.
    /// </summary>
    long LastStoredOffset { get; }

    EvDbStreamAddress StreamAddress { get; }

    /// <summary>
    /// Indicating whether this instance is empty i.e. not having events.
    /// </summary>
    bool IsEmpty { get; }

    int EventsCount { get; }

    JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbView> Views { get; }

    IEnumerable<IEvDbEvent> Events { get; }
    /// <summary>
    /// Freeze events for saving, with locked like mechanism.
    /// </summary>
    /// <returns></returns>
     //Freeze();
}

public interface IEvDbStreamStore
{
    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);
}
