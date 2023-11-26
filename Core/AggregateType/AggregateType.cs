using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Core.Event;
using Core.Aggregate;

namespace Core.AggregateType;


public class AggregateType<StateType> where StateType : notnull, new()
{
    public Dictionary<string, EventType> RegisteredEventTypes { get; private set; } = new();

    public Dictionary<string, IFoldingFunction<StateType>> FoldingLogic = new();

    public Aggregate<StateType> CreateAggregate(string id, List<Event.Event>? events)
    {
        if (events == null)
            return new Aggregate<StateType>(this, id);
        return new Aggregate<StateType>(this, id, events);
    }

    public Aggregate<StateType> CreateAggregate(string id, StateType? snapshot, List<Event.Event>? events)
    {
        return new Aggregate<StateType>(this, id, snapshot, events);
    }

    public void AddEventType(EventType eventType)
    {
        RegisteredEventTypes.Add(eventType.EventTypeName, eventType);
    }

    public void AddFoldingFunction(string eventTypeName, IFoldingFunction<StateType> foldingFunction)
    {
        EventType? eventType;
        if (!RegisteredEventTypes.TryGetValue(eventTypeName, out eventType))
            throw new KeyNotFoundException($"Event type name {eventTypeName} was not found.");
        FoldingLogic.Add(eventTypeName, foldingFunction);

    }

    public void AddEventType(EventType eventType, IFoldingFunction<StateType> foldingFunction)
    {
        AddEventType(eventType);
        AddFoldingFunction(eventType.EventTypeName, foldingFunction);
    }

    public StateType FoldEvents(StateType oldState, List<Event.Event> events)
    {
        StateType currentState = oldState;
        foreach (var e in events)
        {
            currentState = FoldEvent(currentState, e);
        }
        return currentState;
    }

    public StateType FoldEvent(StateType oldState, Event.Event someEvent)
    {
        StateType currentState = oldState;
        IFoldingFunction<StateType>? foldingFunction;
        if (!FoldingLogic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
        currentState = foldingFunction.Fold(currentState, someEvent);
        return currentState;
    }
}
