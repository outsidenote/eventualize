namespace Core.AggregateType;


public class AggregateType
{
    public Type StateType { get; private set; }
    public Dictionary<string,Type> RegisteredEventTypes = new Dictionary<string, Type>();
    AggregateType(Type stateType)
    {
        StateType = stateType;
    }

    // public static Dictionary<Type, FoldingFunction<StateType>> FoldingLogic
    //     = new Dictionary<Type, FoldingFunction<StateType>>();
}
