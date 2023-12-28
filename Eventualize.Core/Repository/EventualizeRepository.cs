using Eventualize.Core;
using Eventualize.Core.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

    async Task<EventualizeAggregate<T>> IEventualizeRepository.GetAsync<T>(
        EventualizeAggregateFactory<T> aggregateFactory,
        string streamId,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        EventualizeStreamUri srmUri = new EventualizeStreamUri(
            aggregateFactory.StreamBaseUri,
            streamId );
        // TODO: [bnaya 2023-12-20] transaction, 
        EventualizeSnapshotUri snapshotUri = new(
            srmUri,
            aggregateFactory.AggregateType);
        IAsyncEnumerable<IEventualizeStoredEvent> events;
        var snapshot = await _storageAdapter.TryGetSnapshotAsync<T>(snapshotUri, cancellation);
        if (snapshot == null)
        {
            EventualizeStreamCursor prm1 = new(snapshotUri, 0);
            events = _storageAdapter.GetAsync(prm1, cancellation);
            return await aggregateFactory.CreateAsync(streamId, events);
        }
        long nextOffset = GetNextOffset(snapshot.Cursor.Offset);
        EventualizeStreamCursor prm2 = new(snapshotUri, nextOffset);
        events = _storageAdapter.GetAsync(prm2, cancellation);
        return await aggregateFactory.CreateAsync(streamId, events, snapshot);
    }

    // TODO: [bnaya 2023-12-28] reduce duplication
    async Task IEventualizeRepository.SaveAsync<T>(EventualizeAggregate<T> aggregate, JsonSerializerOptions? options, CancellationToken cancellation)
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
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, options, cancellation);
        aggregate.ClearPendingEvents();
    }

    async Task IEventualizeRepository.SaveAsync<T>(EventualizeAggregate<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
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
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, jsonTypeInfo, cancellation);
        aggregate.ClearPendingEvents();
    }
}