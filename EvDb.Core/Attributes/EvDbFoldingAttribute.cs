namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbFoldingAttribute
    <TState, TEventAdder> : Attribute
    where TEventAdder : IEvDbEventAdder
{
}