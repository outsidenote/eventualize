using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EvDbStoredSnapshot> TryGetSnapshotAsync(EvDbViewAddress viewAddress, CancellationToken cancellation = default);

    IAsyncEnumerable<IEvDbStoredEvent> GetAsync(EvDbStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync(IEvDbStreamStoreData streamStore, CancellationToken cancellation = default);

    Task<long> GetLastOffsetAsync<T>(EvDbStreamAddress streamAddress, CancellationToken cancellation = default);
}