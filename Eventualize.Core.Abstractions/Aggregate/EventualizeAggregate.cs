using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotSequenceId 

namespace Eventualize.Core;

public class EventualizeAggregate
{
    internal static EventualizeAggregate<T> Create<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots)
                where T : notnull, new()
    {
        EventualizeAggregate<T> aggregate = new( aggregateType, id, minEventsBetweenSnapshots );
        return aggregate;        
    }

    public static async Task<EventualizeAggregate<T>> CreateAsync<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            IAsyncEnumerable<EventualizeEvent> events)
                where T : notnull, new() 
    {
        var result = 
                        await CreateAsync(
                                aggregateType, 
                                id,
                                minEventsBetweenSnapshots, 
                                events, new T());
        return result;
    }

    public static async Task<EventualizeAggregate<T>> CreateAsync<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            IAsyncEnumerable<EventualizeEvent> events,
            T snapshot,
            long snapshotSequenceId = -1)
                where T : notnull, new()
    {
        var (state, count) = await aggregateType.FoldEventsAsync(snapshot, events);
        long lastStoredSequenceId = snapshotSequenceId + count;
        EventualizeAggregate<T> aggregate = new(
                                        state,
                                        aggregateType,
                                        id, 
                                        minEventsBetweenSnapshots,
                                        lastStoredSequenceId);
        return aggregate;        
    }

    public static EventualizeAggregate<T> Create<T>(
            EventualizeAggregateType<T> aggregateType,
            string id,
            int minEventsBetweenSnapshots,
            T snapshot,
            long snapshotSequenceId)
                where T : notnull, new()
    {
        EventualizeAggregate<T> aggregate = new(
                                                snapshot, 
                                                aggregateType, 
                                                id,
                                                minEventsBetweenSnapshots,
                                                snapshotSequenceId);
        return aggregate;        
    }
}

[DebuggerDisplay("LastStoredSequenceId: {LastStoredSequenceId}, State: {State}")]
public class EventualizeAggregate<T> where T : notnull, new()
{
    public string Id { get; private set; }

    public EventualizeAggregateType<T> AggregateType { get; private set; }
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    private ConcurrentQueue<EventualizeEvent> _pendingEvents = new ConcurrentQueue<EventualizeEvent>();
    public IImmutableList<EventualizeEvent> PendingEvents => _pendingEvents.ToImmutableArray();

    public long LastStoredSequenceId { get; private set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; private set; } = 0;

    public T State { get; private set; }

    #region Ctor

    internal EventualizeAggregate(
        EventualizeAggregateType<T> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        long lastStoredSequenceId = -1)
            : this (new T(), aggregateType, id, minEventsBetweenSnapshots, lastStoredSequenceId)
    {
    }

    internal EventualizeAggregate(
        T state,
        EventualizeAggregateType<T> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        long lastStoredSequenceId)
    {
        Id = id;
        State = state;
        AggregateType = aggregateType;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredSequenceId = lastStoredSequenceId; 
    }

    //private EventualizeAggregate(
    //    EventualizeAggregateType<T> aggregateType,
    //    string id,
    //    int minEventsBetweenSnapshots)
    //{
    //    AggregateType = aggregateType;
    //    Id = id;
    //    MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    //    State = new T();
    //}

    //public EventualizeAggregate(
    //    EventualizeAggregateType<T> aggregateType,
    //    string id,
    //    int minEventsBetweenSnapshots,
    //    ICollection<EventualizeEvent> events)
    //        : this(aggregateType, id, minEventsBetweenSnapshots)
    //{
    //    State = AggregateType.FoldEvents(State, events);
    //    LastStoredSequenceId = events.Count - 1;
    //}

    //public EventualizeAggregate(
    //    EventualizeAggregateType<T> aggregateType,
    //    string id,
    //    int minEventsBetweenSnapshots,
    //    ICollection<EventualizeEvent> events)
    //        : this(aggregateType, id, minEventsBetweenSnapshots)
    //{
    //    State = AggregateType.FoldEvents(State, events);
    //    LastStoredSequenceId = events.Count - 1;
    //}

    //public EventualizeAggregate(
    //    EventualizeAggregateType<T> aggregateType,
    //    string id,
    //    int minEventsBetweenSnapshots,
    //    T snapshot,
    //    long snapshotSequenceId,
    //    ICollection<EventualizeEvent> events)
    //        : this(aggregateType, id, minEventsBetweenSnapshots)
    //{
    //    State = AggregateType.FoldEvents(snapshot, events);
    //    LastStoredSequenceId = snapshotSequenceId + events.Count;
    //}

    #endregion // Ctor

    public void AddPendingEvent(EventualizeEvent someEvent)
    {
        EventualizeEventType? eventType;
        if (!AggregateType.RegisteredEventTypes.TryGetValue(someEvent.EventType, out eventType))
            throw new KeyNotFoundException($"No registered event of type {someEvent.EventType}");

        // TODO: [bnaya 2023-12-18] @Roma what is it for?
        eventType.ParseData(someEvent);

        _pendingEvents.Enqueue(someEvent);
        // TODO: [bnaya 2023-12-19] thread safe
        State = AggregateType.FoldEvent(State, someEvent);
    }

    public void ClearPendingEvents()
    {
        LastStoredSequenceId += PendingEvents.Count;
        _pendingEvents.Clear(); 
    }
}

