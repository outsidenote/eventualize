namespace Eventualize.Core;

public interface IEventualizeStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EventualizeStoredSnapshot<T>?> TryGetSnapshotAsync<T>(EventualizeSnapshotUri snapshotUri, CancellationToken cancellation = default) where T : notnull, new();

    IAsyncEnumerable<IEventualizeStoredEvent> GetAsync(EventualizeStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot, CancellationToken cancellation = default) where T : notnull, new();

    Task<long> GetLastOffsetAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation = default) where T : notnull, new();
}