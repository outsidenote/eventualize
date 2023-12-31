using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public interface IEvDbStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EvDbStoredSnapshot<T>?> TryGetSnapshotAsync<T>(EvDbSnapshotUri snapshotUri, CancellationToken cancellation = default) where T : notnull, new();

    IAsyncEnumerable<IEvDbStoredEvent> GetAsync(EvDbStreamCursor parameter, CancellationToken cancellation = default);

    Task SaveAsync<T>(EvDbAggregate<T> aggregate, bool storeSnapshot, JsonSerializerOptions? options = null, CancellationToken cancellation = default) where T : notnull, new();

    Task SaveAsync<T>(EvDbAggregate<T> aggregate, bool storeSnapshot, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation = default) where T : notnull, new();

    Task<long> GetLastOffsetAsync<T>(EvDbAggregate<T> aggregate, CancellationToken cancellation = default) where T : notnull, new();
}