namespace EvDb.Core;

//[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Assembly, AllowMultiple = true)]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAggregateFactoryAttribute<TState, TEventType> : Attribute
    where TEventType : IEvDbEventTypes
{
}