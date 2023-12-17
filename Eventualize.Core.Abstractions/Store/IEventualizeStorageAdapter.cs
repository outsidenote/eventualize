using System.Collections.Immutable;

namespace Eventualize.Core;

public interface IEventualizeStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<EventualizeStoredSnapshotData<T>?> TryGetSnapshotAsync<T>(string aggregateTypeName, string id) where T : notnull, new();
    IAsyncEnumerable<EventualizeEvent> GetAsync(string aggregateTypeName, string id, long startSequenceId = 0);
    Task<IImmutableList<EventualizeEvent>> SaveAsync<T>(EventualizeAggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();
    Task<long> GetLastSequenceIdAsync<T>(EventualizeAggregate<T> aggregate) where T : notnull, new();
}