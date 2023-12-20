using System.Collections.Immutable;

namespace Eventualize.Core;

public interface IEventualizeStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EventualizeStoredSnapshotData<T>?> TryGetSnapshotAsync<T>(AggregateParameter parameter) where T : notnull, new();

    IAsyncEnumerable<EventualizeEvent> GetAsync(AggregateSequenceParameter parameter);

    Task<IImmutableList<EventualizeEvent>> SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();

    Task<long> GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate) where T : notnull, new();
}