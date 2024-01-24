using System.Text.Json;

namespace EvDb.Core;

// TODO: [bnaya 2023-12-10] make it DI friendly (have an interface and DI registration)
internal class EvDbRepository : IEvDbRepository
{
    private readonly IEvDbStorageAdapter _storageAdapter;
    private long _streamOffset = 0;
    private long _snapshotOffset = 0;

    public EvDbRepository(IEvDbStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    async Task<T> IEvDbRepository.GetAsync<T, TState>(
        IEvDbAggregateFactory<T, TState> factory,
        string streamId,
        CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();

        var streamAddress = new EvDbStreamAddress(factory.Partition, streamId);
        // TODO: [bnaya 2023-12-20] transaction, 
        EvDbSnapshotId snapshotId = new EvDbSnapshotId(
            streamAddress,
            factory.Kind);
        EvDbStoredSnapshot<TState>? snapshot = await _storageAdapter.TryGetSnapshotAsync<TState>(snapshotId, cancellation);
        if (snapshot == null)
        {
            EvDbStreamCursor streamCursor = new(streamAddress, 0);
            T agg = factory.Create(streamId);
            var syncNoSnap = (IEvDbCollectionHidden)agg;
            IAsyncEnumerable<IEvDbStoredEvent> allEvents = _storageAdapter.GetAsync(streamCursor, cancellation);
            await foreach (IEvDbStoredEvent e in allEvents)
            {
                syncNoSnap.SyncEvent(e);
            }

            return agg;
        }
        long snapshotOffset = snapshot.Cursor.Offset;
        long nextOffset = _snapshotOffset + 1;
        EvDbStreamCursor prm2 = new(snapshotId, nextOffset);
        IAsyncEnumerable<IEvDbStoredEvent> events = _storageAdapter.GetAsync(prm2, cancellation);
        var result = factory.Create(snapshot);
        var syncSnap = (IEvDbCollectionHidden)result;
        long offset = _streamOffset;
        await foreach (IEvDbStoredEvent e in events)
        {
            syncSnap.SyncEvent(e);
            offset = e.StreamCursor.Offset;
        }
        _streamOffset = offset;
        _snapshotOffset = snapshotOffset;
        return result;
    }

    // TODO: [bnaya 2023-12-28] reduce duplication
    async Task IEvDbRepository.SaveAsync<TState>(
        IEvDbAggregateDeprecated<TState> aggregate,
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

        long snapshotGap = _streamOffset + aggregate.EventsCount - _snapshotOffset;
        bool shouldStoreSnapshot = snapshotGap >= aggregate.MinEventsBetweenSnapshots;
        await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, options, cancellation);
        _streamOffset += aggregate.EventsCount;
        aggregate.ClearLocalEvents(); // TODO: [bnaya 2024-01-09] selective clear is needed
    }

    //async Task IEvDbRepository.SaveAsync<T>(IEvDbAggregateDeprecated<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
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
public class EvDbRepositoryV1 : IEvDbRepositoryV1
{
    private readonly IEvDbStorageAdapter _storageAdapter;

    public EvDbRepositoryV1(IEvDbStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    private static long GetNextOffset(long? offset)
    {
        if (offset == null) return 0;
        return (long)offset + 1;
    }

    async Task<T> IEvDbRepositoryV1.GetAsync<T>(
        IEvDbFactory<T> factory,
        string streamId,
        CancellationToken cancellation)
    {
        throw new NotImplementedException();
        //cancellation.ThrowIfCancellationRequested();

        //var streamAddress = new EvDbStreamAddress(factory.Partition, streamId);
        //// TODO: [bnaya 2023-12-20] transaction, 
        //EvDbSnapshotId snapshotId = new EvDbSnapshotId(
        //    streamAddress,
        //    factory.Kind);
        //EvDbStoredSnapshot? snapshot = await _storageAdapter.TryGetSnapshotAsync(snapshotId, cancellation);
        //if (snapshot == null)
        //{
        //    EvDbStreamCursor streamCursor = new(streamAddress, 0);
        //    T agg = factory.Create(streamId); 
        //    var syncNoSnap = (IEvDbCollectionHidden)agg;
        //    IAsyncEnumerable<IEvDbStoredEvent> allEvents = _storageAdapter.GetAsync(streamCursor, cancellation);
        //    await foreach (IEvDbStoredEvent e in allEvents)
        //    {
        //        syncNoSnap.SyncEvent(e);
        //    }

        //    // TODO: [bnaya 2024-01-09] return agg;?
        //    return agg;
        //}
        //long nextOffset = GetNextOffset(snapshot.Cursor.Offset);
        //EvDbStreamCursor prm2 = new(snapshotId, nextOffset);
        //IAsyncEnumerable<IEvDbStoredEvent> events = _storageAdapter.GetAsync(prm2, cancellation);
        //var result =  factory.Create(snapshot);
        //var syncSnap = (IEvDbCollectionHidden)result;
        //await foreach (IEvDbStoredEvent e in events)
        //{
        //    syncSnap.SyncEvent(e);
        //}
        //return result;
    }

    // TODO: [bnaya 2023-12-28] reduce duplication
    async Task IEvDbRepositoryV1.SaveAsync(
        IEvDbStream aggregate,
        JsonSerializerOptions? options,
        CancellationToken cancellation)
    {
        throw new NotImplementedException();
        //// TODO: [bnaya 2024-01-09] Thread-safe delete/clear only the pending which had saved
        //if (aggregate.IsEmpty)
        //{
        //    await Task.FromResult(true);
        //    return;
        //}
        //// TODO: [bnaya 2024-01-09] TBD: lock/immutable memory-snapshot 


        ////long lastStoredOffset = await _storageAdapter.GetLastOffsetAsync(aggregate, cancellation);
        ////if (lastStoredOffset != aggregate.LastStoredOffset)
        ////    throw new OCCException(aggregate, lastStoredOffset);
        //bool shouldStoreSnapshot = aggregate.EventsCount >= aggregate.MinEventsBetweenSnapshots;
        //await _storageAdapter.SaveAsync(aggregate, shouldStoreSnapshot, options, cancellation);
        //aggregate.ClearLocalEvents(); // TODO: [bnaya 2024-01-09] selective clear is needed
    }

    //async Task IEvDbRepository.SaveAsync<T>(IEvDbAggregateDeprecated<T> aggregate, JsonTypeInfo<T> jsonTypeInfo, CancellationToken cancellation)
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
