namespace Eventualize.Core;

// TODO: [bnaya 2023-12-10] make it DI friendly (have an interface and DI registration)
public class Repository : IRepository
{
    private readonly IEventualizeStorageAdapter _storageAdapter;

    public Repository(IEventualizeStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    private static long GetNextSequenceId(long? sequenceId)
    {
        if (sequenceId == null) return 0;
        return (long)sequenceId + 1;
    }

    public async Task<EventualizeAggregate<T>> GetAsync<T>(EventualizeAggregateType<T> aggregateType, string id) where T : notnull, new()
    {
        IAsyncEnumerable<EventualizeEvent> events;
        var snapshotData = await _storageAdapter.TryGetSnapshotAsync<T>(aggregateType.Name, id);
        if (snapshotData == null)
        {
            events = _storageAdapter.GetAsync(aggregateType.Name, id, 0);
            return await aggregateType.CreateAggregateAsync(id, events);
        }
        long nextSequenceId = GetNextSequenceId(snapshotData.SnapshotSequenceId);
        events = _storageAdapter.GetAsync(aggregateType.Name, id, nextSequenceId);
        return await aggregateType.CreateAggregateAsync(id, events, snapshotData.Snapshot, snapshotData.SnapshotSequenceId);
    }

    public async Task SaveAsync<T>(EventualizeAggregate<T> aggregate) where T : notnull, new()
    {
        if (aggregate.PendingEvents.Count == 0)
        {
            await Task.FromResult(true);
            return;
        }
        long lastStoredSequenceId = await _storageAdapter.GetLastSequenceIdAsync(aggregate);
        if (lastStoredSequenceId != aggregate.LastStoredSequenceId)
            throw new OCCException<T>(aggregate, lastStoredSequenceId);
        bool shouldStoreSnapshot = aggregate.PendingEvents.Count >= aggregate.MinEventsBetweenSnapshots;
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot);
        aggregate.ClearPendingEvents();
    }
}