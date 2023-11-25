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
        ValidateFoldingFunction(eventType, foldingFunction);
        FoldingLogic.Add(eventTypeName, foldingFunction);

    }

    private void ValidateFoldingFunction(EventType eventType, FoldingFunction foldingFunction)
    {
        var methodInfo = foldingFunction.GetMethodInfo();
        Debug.Assert(methodInfo.ReturnType != StateType, $"Folding logic return type if {methodInfo.ReturnType}, but expected {StateType}");
        var parameterInfos = methodInfo.GetParameters();
        Debug.Assert(parameterInfos.Length == 2, "Folding function should have 2 parameters");
        if (StateType == null) throw new ArgumentNullException(nameof(StateType));
        Debug.Assert(parameterInfos[0].GetType() == StateType, "Wrong 1st parameter type in folding function");
        Debug.Assert(parameterInfos[1].GetType() == eventType.DataSchemaType, "Wrong 2nd parameter type in folding function");

    }

    public void AddEventType(EventType eventType, FoldingFunction foldingFunction)
    {
        AddEventType(eventType);
        AddFoldingFunction(eventType.EventTypeName, foldingFunction);
    }


    // public static Dictionary<Type, FoldingFunction<StateType>> FoldingLogic
    //     = new Dictionary<Type, FoldingFunction<StateType>>();
}
