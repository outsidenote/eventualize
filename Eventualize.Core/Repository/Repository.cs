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

    async Task<EventualizeAggregate<T>> IRepository.GetAsync<T>(EventualizeAggregateFactory<T> aggregateFactory, string streamId, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        // TODO: [bnaya 2023-12-20] transaction, 
        string type = aggregateFactory.StreamBaseAddress.StreamType;
        AggregateParameter parameter = new(streamId, type);
        IAsyncEnumerable<EventualizeStoredEvent> events;
        var snapshotData = await _storageAdapter.TryGetSnapshotAsync<T>(parameter, cancellation);
        if (snapshotData == null)
        {
            AggregateSequenceParameter prm1 = new(parameter, 0);
            events = _storageAdapter.GetAsync(prm1, cancellation);
            return await aggregateFactory.CreateAsync(streamId, events);
        }
        long nextSequenceId = GetNextSequenceId(snapshotData.SnapshotSequenceId);
        AggregateSequenceParameter prm2 = new(parameter, nextSequenceId);
        events = _storageAdapter.GetAsync(prm2, cancellation);
        return await aggregateFactory.CreateAsync(streamId, events, snapshotData.Snapshot);
    }

    async Task IRepository.SaveAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation)
    {
        if (aggregate.PendingEvents.Count == 0)
        {
            await Task.FromResult(true);
            return;
        }
        long lastStoredSequenceId = await _storageAdapter.GetLastSequenceIdAsync(aggregate, cancellation);
        if (lastStoredSequenceId != aggregate.LastStoredSequenceId)
            throw new OCCException<T>(aggregate, lastStoredSequenceId);
        bool shouldStoreSnapshot = aggregate.PendingEvents.Count >= aggregate.MinEventsBetweenSnapshots;
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, cancellation);
        aggregate.ClearPendingEvents();
    }
}