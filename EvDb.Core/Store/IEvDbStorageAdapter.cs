using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EvDbStoredSnapshot<T>?> TryGetSnapshotAsync<T>(EvDbViewAddress snapshotId, CancellationToken cancellation = default);

    IAsyncEnumerable<IEvDbStoredEvent> GetAsync(EvDbStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync<T>(IEvDbStreamStore streamStore, IEnumerable<IEvDbView> views, bool storeSnapshot, JsonSerializerOptions? options = null, CancellationToken cancellation = default);

    Task<long> GetLastOffsetAsync<T>(EvDbStreamAddress streamAddress, CancellationToken cancellation = default);
}