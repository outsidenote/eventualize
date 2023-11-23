namespace Core.AggregateType;


public class AggregateType<StateType>
{
    public static Dictionary<Type, FoldingFunction<StateType>> FoldingLogic
        = new Dictionary<Type, FoldingFunction<StateType>>();
}
