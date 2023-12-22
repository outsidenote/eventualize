// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotSequenceId 

using Eventualize.Core.Abstractions.Stream;

namespace Eventualize.Core;

public class EventualizeAggregateFactory<T> where T : notnull, new()
{
    #region Members
    public readonly Dictionary<string, EventualizeEventType> RegisteredEventTypes;
    public readonly EventualizeFoldingLogic<T> FoldingLogic;

    public readonly string AggregateType;
    public readonly EventualizeStreamBaseAddress StreamBaseAddress;
    public readonly int MinEventsBetweenSnapshots;

    #endregion // Members

    public EventualizeAggregateFactory(string aggregateType, EventualizeStreamBaseAddress streamBaseAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic, int minEventsBetweenSnapshots)
    {
        AggregateType = aggregateType;
        StreamBaseAddress = streamBaseAddress;
        RegisteredEventTypes = registeredEventTypes;
        FoldingLogic = foldingLogic;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }
    public EventualizeAggregateFactory(string aggregateType, EventualizeStreamBaseAddress streamBaseAddress, Dictionary<string, EventualizeEventType> registeredEventTypes, EventualizeFoldingLogic<T> foldingLogic)
        : this(aggregateType, streamBaseAddress, registeredEventTypes, foldingLogic, 0) { }

    internal EventualizeAggregate<T> Create(string id)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamAddress(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots);
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(string id, IAsyncEnumerable<EventualizeStoredEvent> storedEvents)
    {
        return await CreateAsync(id, storedEvents, new T());
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(
            string id,
            IAsyncEnumerable<EventualizeStoredEvent> storedEvents,
            T snapshot)
    {
        long sequenceId = -1;
        T state = snapshot;
        await foreach (var e in storedEvents)
        {
            state = FoldingLogic.FoldEvent(state, e);
            sequenceId = e.SequenceId;
        }
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamAddress(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots, state, sequenceId);
    }

    public EventualizeAggregate<T> Create(string id, T snapshot, long snapshotSequenceId)
    {
        return new EventualizeAggregate<T>(AggregateType, new EventualizeStreamAddress(StreamBaseAddress, id), RegisteredEventTypes, FoldingLogic, MinEventsBetweenSnapshots, snapshot, snapshotSequenceId);
    }
}

