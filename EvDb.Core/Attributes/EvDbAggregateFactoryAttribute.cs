namespace EvDb.Core;

[Obsolete("Deprecated")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAggregateFactoryAttribute<TState, TEventType> : Attribute
    where TEventType : IEvDbEventAdder
{
}


/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbStreamFactoryAttribute<TEventType> : Attribute
    where TEventType : IEvDbEventAdder
{
    public string? CollectionName { get; set; } 
}