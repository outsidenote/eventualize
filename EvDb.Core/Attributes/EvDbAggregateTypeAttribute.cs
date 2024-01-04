namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbAggregateTypeAttribute<TState, TEventType> : Attribute
{
    public EvDbAggregateTypeAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}