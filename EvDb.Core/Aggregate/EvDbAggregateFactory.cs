// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset

namespace EvDb.Core;

public class EvDbAggregateFactory<T> where T : notnull, new()
{
    #region Members
    public readonly EvDbFoldingLogic<T> FoldingLogic;

    public readonly string AggregateType;
    public readonly EvDbStreamBaseUri StreamBaseUri;
    public readonly int MinEventsBetweenSnapshots;

    #endregion // Members

    public EvDbAggregateFactory(
        string aggregateType,
        EvDbStreamBaseUri streamBaseAddress,
        EvDbFoldingLogic<T> foldingLogic,
        int minEventsBetweenSnapshots = 0)
    {
        AggregateType = aggregateType;
        StreamBaseUri = streamBaseAddress;
        FoldingLogic = foldingLogic;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }

    public EvDbAggregate<T> Create(string id)
    {
        return new EvDbAggregate<T>(AggregateType, new EvDbStreamUri(StreamBaseUri, id), FoldingLogic, MinEventsBetweenSnapshots);
    }

    public async Task<EvDbAggregate<T>> CreateAsync(string id, IAsyncEnumerable<IEvDbStoredEvent> storedEvents)
    {
        var snap = EvDbStoredSnapshot.Create<T>();
        return await CreateAsync(id, storedEvents, snap);
    }

    public async Task<EvDbAggregate<T>> CreateAsync(
            string id,
            IAsyncEnumerable<IEvDbStoredEvent> storedEvents,
            EvDbStoredSnapshot<T> snapshot)
    {
        long offset = snapshot.Cursor.Offset;
        T state = snapshot.State;
        await foreach (var e in storedEvents)
        {
            state = FoldingLogic.FoldEvent(state, e);
            offset = e.StreamCursor.Offset;
        }
        return new EvDbAggregate<T>(AggregateType, new EvDbStreamUri(StreamBaseUri, id), FoldingLogic, MinEventsBetweenSnapshots, state, offset);
    }

    public EvDbAggregate<T> Create(string id, T snapshot, long snapshotOffset)
    {
        return new EvDbAggregate<T>(AggregateType, new EvDbStreamUri(StreamBaseUri, id), FoldingLogic, MinEventsBetweenSnapshots, snapshot, snapshotOffset);
    }
}

