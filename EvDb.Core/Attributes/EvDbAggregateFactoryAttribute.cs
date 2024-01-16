using System.Text.Json;

namespace EvDb.Core;

[Obsolete("Deprecated")]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbAggregateFactoryAttribute<TState, TEventType> : Attribute
    where TEventType : IEvDbEventTypes
{
}


/// <summary>
/// Generate a EventualizeDB client factory
/// </summary>
/// <typeparam name="TEventType">The type of the event type.</typeparam>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbFactoryAttribute<TEventType> : Attribute
    where TEventType : IEvDbEventTypes
{ 
}