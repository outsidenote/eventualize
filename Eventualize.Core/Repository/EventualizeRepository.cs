using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

// TODO: [bnaya 2023-12-10] make it DI friendly (have an interface and DI registration)
public class EventualizeRepository : IEventualizeRepository
{
    private readonly IEventualizeStorageAdapter _storageAdapter;

    public EventualizeRepository(IEventualizeStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    private static long GetNextOffset(long? offset)
    {
        if (offset == null) return 0;
        return (long)offset + 1;
    }

    async Task<EventualizeAggregate<T>> IEventualizeRepository.GetAsync<T>(EventualizeAggregateFactory<T> aggregateFactory, string streamId, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        // TODO: [bnaya 2023-12-20] transaction, 
        string type = aggregateFactory.StreamBaseAddress.StreamType;
        EventualizeStreamUri streamUri = new(aggregateFactory.StreamBaseAddress, streamId);
        IAsyncEnumerable<IEventualizeStoredEvent> events;
        var snapshotData = await _storageAdapter.TryGetSnapshotAsync<T>(streamUri, cancellation);
        if (snapshotData == null)
        {
            EventualizeStreamCursor prm1 = new(streamUri, 0);
            events = _storageAdapter.GetAsync(prm1, cancellation);
            return await aggregateFactory.CreateAsync(streamId, events);
        }
        long nextOffset = GetNextOffset(snapshotData.SnapshotOffset);
        EventualizeStreamCursor prm2 = new(streamUri, nextOffset);
        events = _storageAdapter.GetAsync(prm2, cancellation);
        return await aggregateFactory.CreateAsync(streamId, events, snapshotData);
    }

    async Task IEventualizeRepository.SaveAsync<T>(EventualizeAggregate<T> aggregate, CancellationToken cancellation)
    {
        if (aggregate.PendingEvents.Count == 0)
        {
            await Task.FromResult(true);
            return;
        }
        long lastStoredOffset = await _storageAdapter.GetLastOffsetAsync(aggregate, cancellation);
        if (lastStoredOffset != aggregate.LastStoredOffset)
            throw new OCCException<T>(aggregate, lastStoredOffset);
        bool shouldStoreSnapshot = aggregate.PendingEvents.Count >= aggregate.MinEventsBetweenSnapshots;
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, cancellation);
        aggregate.ClearPendingEvents();
    }
}