using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EvDbStoredSnapshot<T>?> TryGetSnapshotAsync<T>(EvDbSnapshotId snapshotId, CancellationToken cancellation = default);

    IAsyncEnumerable<IEvDbStoredEvent> GetAsync(EvDbStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync<T>(IEvDbAggregateDeprecated<T> aggregate, bool storeSnapshot, JsonSerializerOptions? options = null, CancellationToken cancellation = default);

    Task<long> GetLastOffsetAsync<T>(IEvDbAggregateDeprecated<T> aggregate, CancellationToken cancellation = default);
}