using System.IO;
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

    async Task<T> IEvDbRepository.GetAsync<T, TState>(
        IEvDbAggregateFactory<T, TState> factory,
        EvDbStreamAddress streamAddress,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        // TODO: [bnaya 2023-12-20] transaction, 
        EvDbSnapshotId snapshotId = new(
            streamAddress,
            factory.Kind);
        string streamId = streamAddress.StreamId;
        EvDbStoredSnapshot<TState>? snapshot = await _storageAdapter.TryGetSnapshotAsync<TState>(snapshotId, cancellation);
        if (snapshot == null)
        {
            EvDbStreamCursor prm1 = new(snapshotId, 0);
            T agg = factory.Create(streamId); // TODO: [bnaya 2024-01-09] TBD: lastStoredOffset?
            var pub = (IEvDbEventPublisher)agg;
            IAsyncEnumerable<IEvDbStoredEvent> snapEvents = _storageAdapter.GetAsync(prm1, cancellation);
            await foreach (IEvDbStoredEvent e in snapEvents)
            {
                agg.AddEvent(e);
            }

            // TODO: [bnaya 2024-01-09] return agg;?
            return agg;
        }
        long nextOffset = GetNextOffset(snapshot.Cursor.Offset);
        EvDbStreamCursor prm2 = new(snapshotId, nextOffset);
        IAsyncEnumerable<IEvDbStoredEvent> events = _storageAdapter.GetAsync(prm2, cancellation);
        var result =  factory.Create(snapshot);
        await foreach (IEvDbStoredEvent e in events)
        {
            result.AddEvent(e);
        }
        return result;
    }

    // TODO: [bnaya 2023-12-28] reduce duplication
    async Task IEvDbRepository.SaveAsync<TState>(
        IEvDbAggregate<TState> aggregate, 
        JsonSerializerOptions? options,
        CancellationToken cancellation)
    {
        // TODO: [bnaya 2024-01-09] Thread-safe delete/clear only the pending which had saved
        if (aggregate.IsEmpty)
        {
            await Task.FromResult(true);
            return;
        }
        // TODO: [bnaya 2024-01-09] TBD: lock/immutable memory-snapshot 


        //long lastStoredOffset = await _storageAdapter.GetLastOffsetAsync(aggregate, cancellation);
        //if (lastStoredOffset != aggregate.LastStoredOffset)
        //    throw new OCCException(aggregate, lastStoredOffset);
        bool shouldStoreSnapshot = aggregate.EventsCount >= aggregate.MinEventsBetweenSnapshots;
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, options, cancellation);
        aggregate.ClearLocalEvents(); // TODO: [bnaya 2024-01-09] selective clear is needed
    }

    //async Task IEvDbRepository.SaveAsync<T>(IEvDbAggregate<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
    //{
    //    if (aggregate.IsEmpty)
    //    {
    //        await Task.FromResult(true);
    //        return;
    //    }
    //    long lastStoredOffset = await _storageAdapter.GetLastOffsetAsync(aggregate, cancellation);
    //    if (lastStoredOffset != aggregate.LastStoredOffset)
    //        throw new OCCException(aggregate, lastStoredOffset);
    //    bool shouldStoreSnapshot = aggregate.EventsCount >= aggregate.MinEventsBetweenSnapshots;
    //    await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, jsonTypeInfo, cancellation);
    //    aggregate.ClearEvents();
    //}
}