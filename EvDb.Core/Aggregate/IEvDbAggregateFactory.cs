
namespace EvDb.Core;

public interface IEvDbAggregateFactory<T, TState>
    where T : IEvDbAggregate<TState>, IEvDbEventTypes
{
    EvDbPartitionAddress Partition { get; }

    string Kind { get; }

    T Create(string streamId,
             long lastStoredOffset = -1);
    T Create(EvDbStoredSnapshot<TState> snapshot);

    Task<T> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default);
}