using System.Collections.Immutable;

namespace Eventualize.Core;

public interface IEventualizeStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EventualizeStoredSnapshotData<T>?> TryGetSnapshotAsync<T>(AggregateParameter parameter, CancellationToken cancellation = default) where T : notnull, new();

    IAsyncEnumerable<EventualizeEvent> GetAsync(AggregateSequenceParameter parameter, CancellationToken cancellation = default);

    Task<IImmutableList<EventualizeEvent>> SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation = default) where T : notnull, new();

    Task<long> GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation = default) where T : notnull, new();
}