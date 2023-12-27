// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset

using Eventualize.Core;

namespace Eventualize.Core;

public class EventualizeAggregateFactory<T> where T : notnull, new()
{
    #region Members
    public readonly Dictionary<string, EventualizeEventType> RegisteredEventTypes;
    public readonly EventualizeFoldingLogic<T> FoldingLogic;

    public readonly string AggregateType;
    public readonly EventualizeStreamBaseUri StreamBaseAddress;
    public readonly int MinEventsBetweenSnapshots;

    #endregion // Members

    public EventualizeAggregateFactory(string aggregateType, EventualizeStreamBaseUri streamBaseAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic, int minEventsBetweenSnapshots)
    {
        AggregateType = aggregateType;
        StreamBaseAddress = streamBaseAddress;
        RegisteredEventTypes = registeredEventTypes;
        FoldingLogic = foldingLogic;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }
    public EventualizeAggregateFactory(string aggregateType, EventualizeStreamBaseUri streamBaseAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic)
        : this(aggregateType, streamBaseAddress, registeredEventTypes, foldingLogic, 0) { }

    public EventualizeAggregate<T> Create(string id)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots);
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(string id, IAsyncEnumerable<EventualizeStoredEvent> storedEvents)
    {
        var snap = EventualizeStoredSnapshot<T>.Create();
        return await CreateAsync(id, storedEvents, snap);
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(
            string id,
            IAsyncEnumerable<EventualizeStoredEvent> storedEvents,
            EventualizeStoredSnapshot<T> snapshot)
    {
        long offset = snapshot.Cursor.Offset;
        T state = snapshot.State;
        await foreach (var e in storedEvents)
        {
            state = FoldingLogic.FoldEvent(state, e);
            offset = e.Offset;
        }
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots, state, offset);
    }

    public EventualizeAggregate<T> Create(string id, T snapshot, long snapshotOffset)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamUri(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots, snapshot, snapshotOffset);
    }
}

