namespace EvDb.Core;

[Obsolete("Deprecated")]
public interface IEvDbAggregateFactory<T, TState>
    where T : IEvDbAggregateDeprecated<TState>, IEvDbEventAdder
{
    EvDbPartitionAddress Partition { get; }

    string Kind { get; }

    T Create(string streamId,
             long lastStoredOffset = -1);
    T Create(EvDbStoredSnapshot<TState> snapshot);

    Task<T> GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default);
}
