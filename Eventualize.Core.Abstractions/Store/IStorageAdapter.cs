namespace Eventualize.Core;

public interface IStorageAdapter
{
    // TODO: [bnaya 2023-12-07] should do it internally, the user shouldn't be aware of it (implementation details)
    [Obsolete("deprecated")]
    public Task Init();
    public Task<StoredSnapshotData<T>?> TryGetSnapshotAsync<T>(string aggregateTypeName, string id) where T : notnull, new();
    public Task<List<EventEntity>> GetAsync(string aggregateTypeName, string id, long startSequenceId);
    public Task<List<EventEntity>?> SaveAsync<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new();
    public Task<long> GetLastSequenceIdAsync<T>(Aggregate<T> aggregate) where T : notnull, new();
}