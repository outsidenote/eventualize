namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbFoldingAttribute
    <TState, TEventType> : Attribute
    where TEventType : IEvDbEventTypes
{
}