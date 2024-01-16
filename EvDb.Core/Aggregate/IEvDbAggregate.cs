using System.Collections.Immutable;
using System.Text.Json;
using System;
namespace EvDb.Core;

public interface IEvDbAggregate
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



public interface IEvDbAggregate<out TState> : IEvDbAggregate
{
    TState State { get; }

    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);

    // TODO: [bnaya 2024-01-09] selective clear is needed
    void ClearLocalEvents();

    //Task GetAsync<TState>(string streamId, CancellationToken cancellation = default);
}

public interface IEvDb : IEvDbAggregate
{
    /// <summary>
    /// Saves pending events into the injected storage.
    /// </summary>
    /// <param name="cancellation">The cancellation.</param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);

    // TODO: [bnaya 2024-01-09] selective clear is needed
    void ClearLocalEvents();

    //Task GetAsync<TState>(string streamId, CancellationToken cancellation = default);
}
