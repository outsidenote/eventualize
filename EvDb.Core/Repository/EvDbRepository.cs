using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

// TODO: [bnaya 2023-12-10] make it DI friendly (have an interface and DI registration)
public class EvDbRepository : IEvDbRepository
{
    private readonly IEvDbStorageAdapter _storageAdapter;

    public EvDbRepository(IEvDbStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    private static long GetNextOffset(long? offset)
    {
        if (offset == null) return 0;
        return (long)offset + 1;
    }

    async Task<EvDbAggregate<T>> IEvDbRepository.GetAsync<T>(
        EvDbAggregateFactory<T> aggregateFactory,
        string streamId,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        EvDbStreamId srmId = new EvDbStreamId(
            aggregateFactory.StreamType,
            streamId);
        // TODO: [bnaya 2023-12-20] transaction, 
        EvDbSnapshotId snapshotId = new(
            srmId,
            aggregateFactory.AggregateType);
        IAsyncEnumerable<IEvDbStoredEvent> events;
        var snapshot = await _storageAdapter.TryGetSnapshotAsync<T>(snapshotId, cancellation);
        if (snapshot == null)
        {
            EvDbStreamCursor prm1 = new(snapshotId, 0);
            events = _storageAdapter.GetAsync(prm1, cancellation);
            return await aggregateFactory.CreateAsync(streamId, events);
        }
        long nextOffset = GetNextOffset(snapshot.Cursor.Offset);
        EvDbStreamCursor prm2 = new(snapshotId, nextOffset);
        events = _storageAdapter.GetAsync(prm2, cancellation);
        return await aggregateFactory.CreateAsync(streamId, events, snapshot);
    }

    // TODO: [bnaya 2023-12-28] reduce duplication
    async Task IEvDbRepository.SaveAsync<T>(EvDbAggregate<T> aggregate, JsonSerializerOptions? options, CancellationToken cancellation)
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

    async Task IEvDbRepository.SaveAsync<T>(EvDbAggregate<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
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