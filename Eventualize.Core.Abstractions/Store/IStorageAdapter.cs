namespace Eventualize.Core;

public interface IStorageAdapter : IDisposable, IAsyncDisposable
{
    Task<StoredSnapshotData<T>?> TryGetSnapshotAsync<T>(string aggregateTypeName, string id) where T : notnull, new();
    Task<List<EventEntity>> GetAsync(string aggregateTypeName, string id, long startSequenceId);
    Task<List<EventEntity>?> SaveAsync<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();
    Task<long> GetLastSequenceIdAsync<T>(Aggregate<T> aggregate) where T : notnull, new();
}