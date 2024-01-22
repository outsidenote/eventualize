namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbViewAttribute
    <TState, TEventAdder> : Attribute
    where TEventAdder : IEvDbEventAdder
{
    public string? PropertyName { get; set; }
}