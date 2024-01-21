
using System.Text.Json;

namespace EvDb.Core;

[Obsolete("Deprecated")]
public interface IEvDbAggregateFactory<T, TState>
    where T : IEvDbAggregate<TState>, IEvDbEventAdder
{
    EvDbPartitionAddress Partition { get; }

    string Kind { get; }

    T Create(string streamId,
             long lastStoredOffset = -1);
    T Create(EvDbStoredSnapshot<TState> snapshot);

    Task<T> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default);
}

public interface IEvDbFactory
{
    EvDbPartitionAddress Partition { get; }
    string Kind { get; }

    int MinEventsBetweenSnapshots { get; }

    JsonSerializerOptions? JsonSerializerOptions { get; }
}

public interface IEvDbFactory<T> : IEvDbFactory
    where T : IEvDb, IEvDbEventAdder
{

    T Create(string streamId,
             long lastStoredOffset = -1);
    // T Create(EvDbStoredSnapshot<TState> snapshot);

    Task<T> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default);
}