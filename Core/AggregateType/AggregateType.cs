using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Core.Event;

namespace Core.AggregateType;


public class AggregateType
{
    public Type StateType { get; private set; }
    public Dictionary<string, EventType> RegisteredEventTypes { get; private set; }
        = new Dictionary<string, EventType>();

    public Dictionary<string, FoldingFunction> FoldingLogic = new Dictionary<string, FoldingFunction>();
    public AggregateType(Type stateType)
    {
        StateType = stateType;
    }

    public void AddEventType(EventType eventType)
    {
        RegisteredEventTypes.Add(eventType.EventTypeName, eventType);
    }

    public void AddFoldingFunction(string eventTypeName, FoldingFunction foldingFunction)
    {
        EventType? eventType;
        if (!RegisteredEventTypes.TryGetValue(eventTypeName, out eventType))
            throw new KeyNotFoundException($"Event type name {eventTypeName} was not found.");
        // ValidateFoldingFunction(eventType, foldingFunction);
        FoldingLogic.Add(eventTypeName, foldingFunction);

    }

    public void AddEventType(EventType eventType, FoldingFunction foldingFunction)
    {
        AddEventType(eventType);
        AddFoldingFunction(eventType.EventTypeName, foldingFunction);
    }

    public dynamic FoldEvents(object oldState, List<Event.Event> events)
    {
        object currentState = oldState;
        foreach (var e in events)
        {
            FoldingFunction? foldingFunction;
            if (!FoldingLogic.TryGetValue(e.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(FoldingLogic));
            currentState = foldingFunction(currentState, e);
        }
        return currentState;
    }
}
