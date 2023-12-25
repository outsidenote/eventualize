// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset

using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

public class EventualizeAggregateFactory<T> where T : notnull, new()
{
    #region Members
    public readonly EventualizeFoldingLogic<T> FoldingLogic;

    public readonly string AggregateType;
    public readonly EventualizeStreamBaseUri StreamBaseAddress;
    public readonly int MinEventsBetweenSnapshots;

    #endregion // Members

    public EventualizeAggregateFactory(string aggregateType, EventualizeStreamBaseUri streamBaseAddress, EventualizeFoldingLogic<T> foldingLogic, int minEventsBetweenSnapshots = 0)
    {
        AggregateType = aggregateType;
        StreamBaseAddress = streamBaseAddress;
        FoldingLogic = foldingLogic;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }

    public EventualizeAggregate<T> Create(string id)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id),  FoldingLogic, MinEventsBetweenSnapshots);
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(string id, IAsyncEnumerable<IEventualizeStoredEvent> storedEvents)
    {
        var snap = EventualizeStoredSnapshotData<T>.Create();
        return await CreateAsync(id, storedEvents, snap);
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(
            string id,
            IAsyncEnumerable<IEventualizeStoredEvent> storedEvents,
            EventualizeStoredSnapshotData<T> snapshotData)
    {
        long offset = snapshotData.SnapshotOffset;
        T state = snapshotData.Snapshot;
        await foreach (var e in storedEvents)
        {
            state = FoldingLogic.FoldEvent(state, e);
            offset = e.StreamCursor.Offset;
        }
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id),  FoldingLogic, MinEventsBetweenSnapshots, state, offset);
    }

    public EventualizeAggregate<T> Create(string id, T snapshot, long snapshotOffset)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id),  FoldingLogic, MinEventsBetweenSnapshots, snapshot, snapshotOffset);
    }
}

