using System.Collections.Immutable;
using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

public interface IEventualizeStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EventualizeStoredSnapshotData<T>?> TryGetSnapshotAsync<T>(EventualizeStreamUri streamUri, CancellationToken cancellation = default) where T : notnull, new();

    IAsyncEnumerable<IEventualizeStoredEvent> GetAsync(EventualizeStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation = default) where T : notnull, new();

    Task<long> GetLastOffsetAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation = default) where T : notnull, new();
}