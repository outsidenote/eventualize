using System.Text.Json;
namespace EvDb.Core;

public interface IEvDbCollectionMeta
{
    /// <summary>
    /// The purpose of the aggregation.
    /// </summary>
    string Kind { get; }

    /// <summary>
    /// Bookmark to the last successful event persistence.
    /// </summary>
    long LastStoredOffset { get; }

    /// <summary>
    /// Throttle snapshot persistence.
    /// </summary>
    int MinEventsBetweenSnapshots { get; }

    EvDbStreamAddress StreamId { get; }

    EvDbSnapshotId SnapshotId { get; }

    /// <summary>
    /// Indicating whether this instance is empty i.e. not having events.
    /// </summary>
    bool IsEmpty { get; }

    int EventsCount { get; }

    JsonSerializerOptions? Options { get; }

    [Obsolete("should be part of a memory snapshot")]
    IEnumerable<IEvDbEvent> Events { get; }
    /// <summary>
    /// Freeze events for saving, with locked like mechanism.
    /// </summary>
    /// <returns></returns>
     //Freeze();
}
