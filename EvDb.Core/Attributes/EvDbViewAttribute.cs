namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbViewAttribute
    <TState, TEventAdder> : Attribute
    where TEventAdder : IEvDbEventAdder
{
}