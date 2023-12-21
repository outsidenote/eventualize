using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotSequenceId 

namespace Eventualize.Core;

[DebuggerDisplay("LastStoredSequenceId: {LastStoredSequenceId}, State: {State}")]
public abstract class EventualizeAggregate
{
    #region Ctor

    internal EventualizeAggregate(
        string id,
        int minEventsBetweenSnapshots,
        long lastStoredSequenceId)
    {
        Id = id;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredSequenceId = lastStoredSequenceId;
    }

    #endregion // Ctor

    public string Id { get; }

    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal ConcurrentQueue<EventualizeEvent> _pendingEvents = new ConcurrentQueue<EventualizeEvent>();
    public IImmutableList<EventualizeEvent> PendingEvents => _pendingEvents.ToImmutableArray();

    public long LastStoredSequenceId { get; protected set; } = -1;
    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots
    public int MinEventsBetweenSnapshots { get; } = 0;

    public abstract string Type { get; }
}

[DebuggerDisplay("LastStoredSequenceId: {LastStoredSequenceId}, State: {State}")]
public class EventualizeAggregate<T>: EventualizeAggregate where T : notnull, new()
{
    #region Ctor

    internal EventualizeAggregate(
        EventualizeAggregateType<T> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        long lastStoredSequenceId = -1)
            : this(new T(), aggregateType, id, minEventsBetweenSnapshots, lastStoredSequenceId)
    {
    }

    internal EventualizeAggregate(
        T state,
        EventualizeAggregateType<T> aggregateType,
        string id,
        int minEventsBetweenSnapshots,
        long lastStoredSequenceId) 
        : base(id, minEventsBetweenSnapshots, lastStoredSequenceId)
    {
        State = state;
        _aggregateType = aggregateType;
    }

    #endregion // Ctor

    public async Task<EventualizeAggregate<T>> CreateAsync(string id, IAsyncEnumerable<EventualizeEvent> events)
    {
        var result = await EventualizeAggregateFactory.CreateAsync(
                                _aggregateType,
                                id,
                                MinEventsBetweenSnapshots,
                                events);
        return result;
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(IAsyncEnumerable<EventualizeEvent> events)
    {
        var result = await CreateAsync(Id, events);
        return result;
    }

    public async Task<EventualizeAggregate<T>> CreateAsync(
                    string id,
                    IAsyncEnumerable<EventualizeEvent> events, 
                    T snapshot,
                    long lastStoredSequenceId)
    {
        var result = await EventualizeAggregateFactory.CreateAsync(
                                   _aggregateType,
                                   id,
                                   MinEventsBetweenSnapshots,
                                   events,
                                   snapshot,
                                   lastStoredSequenceId);
        return result;
    }

    private readonly EventualizeAggregateType<T> _aggregateType;
 
    public T State { get; private set; }

    public override string Type => _aggregateType.Name;

    public void AddPendingEvent(EventualizeEvent someEvent)
    {
        EventualizeEventType? eventType;
        if (!_aggregateType.RegisteredEventTypes.TryGetValue(someEvent.EventType, out eventType))
            throw new KeyNotFoundException($"No registered event of type {someEvent.EventType}");

        // TODO: [bnaya 2023-12-18] @Roma what is it for?
        eventType.ParseData(someEvent);

        _pendingEvents.Enqueue(someEvent);
        // TODO: [bnaya 2023-12-19] thread safe
        State = _aggregateType.FoldEvent(State, someEvent);
    }

    public void ClearPendingEvents()
    {
        LastStoredSequenceId += PendingEvents.Count;
        _pendingEvents.Clear();
    }
}

