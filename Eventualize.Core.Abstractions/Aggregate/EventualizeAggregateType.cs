namespace Eventualize.Core;

public class EventualizeAggregateType<T> where T : notnull, new()
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    public Dictionary<string, EventualizeEventType> RegisteredEventTypes { get; private set; } = new();

    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    public Dictionary<string, IFoldingFunction<T>> FoldingLogic = new();

    public readonly string Name;

    public readonly int MinEventsBetweenSnapshots;  

    public EventualizeAggregateType(string name)
    {
        Name = name;
        MinEventsBetweenSnapshots = 0;
    }

    public EventualizeAggregateType(string name, int minEventsBetweenSnapshots)
    {
        Name = name;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
    }

    public EventualizeAggregate<T> CreateAggregate(string id)
    {
        return EventualizeAggregate.Create(this, id, MinEventsBetweenSnapshots);
    }

    public async Task<EventualizeAggregate<T>> CreateAggregateAsync(
                                string id,
                                IAsyncEnumerable<EventualizeEvent> events)
    {
        var result = await EventualizeAggregate.CreateAsync(
                                this,
                                id, 
                                MinEventsBetweenSnapshots, 
                                events);
        return result;
    }

    public EventualizeAggregate<T> CreateAggregate(
                    string id,
                    T snapshot,
                    long lastStoredSequenceId = 0)
    {
        var result = EventualizeAggregate.Create(
                                    this,
                                    id,
                                    MinEventsBetweenSnapshots,
                                    snapshot,
                                    lastStoredSequenceId);
        return result;
    }
    public async Task<EventualizeAggregate<T>> CreateAggregateAsync(
                    string id,
                    IAsyncEnumerable<EventualizeEvent> events,
                    T snapshot,
                    long lastStoredSequenceId)
    {
        var result = await EventualizeAggregate.CreateAsync(
                                    this,
                                    id,
                                    MinEventsBetweenSnapshots, 
                                    events,
                                    snapshot, 
                                    lastStoredSequenceId );
        return result;
    }

    public void AddEventType(EventualizeEventType eventType)
    {
        RegisteredEventTypes.Add(eventType.EventTypeName, eventType);
    }

    public void AddFoldingFunction(
                            string eventTypeName, 
                            IFoldingFunction<T> foldingFunction)
    {
        EventualizeEventType? eventType;
        if (!RegisteredEventTypes.TryGetValue(eventTypeName, out eventType))
            throw new KeyNotFoundException($"Event type name {eventTypeName} was not found.");
        FoldingLogic.Add(eventTypeName, foldingFunction);

    }

    public void AddEventType(
                        EventualizeEventType eventType, 
                        IFoldingFunction<T> foldingFunction)
    {
        AddEventType(eventType);
        AddFoldingFunction(eventType.EventTypeName, foldingFunction);
    }

    [Obsolete("deprecated", true)]
    public T FoldEvents(T oldState, IEnumerable<EventualizeEvent> events)
    {
        T currentState = oldState;
        foreach (var e in events)
        {
            currentState = FoldEvent(currentState, e);
        }
        return currentState;
    }

    public async Task<FoldingResult<T>> FoldEventsAsync(
        IAsyncEnumerable<EventualizeEvent> events)
    {
        T state = new();
        var result = await FoldEventsAsync(state, events);
        return result;
    }

    public async Task<FoldingResult<T>> FoldEventsAsync(
        T oldState,
        IAsyncEnumerable<EventualizeEvent> events)
    {
        long count = 0;
        T currentState = oldState;
        await foreach (var e in events)
        {
            currentState = FoldEvent(currentState, e);
            count++;
        }
        return new FoldingResult<T>(currentState, count);
    }

    public T FoldEvent(T oldState, EventualizeEvent someEvent)
    {
        T currentState = oldState;
        IFoldingFunction<T>? foldingFunction;
        if (!FoldingLogic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
        currentState = foldingFunction.Fold(currentState, someEvent);
        return currentState;
    }
}
