#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbViewAttribute
    <TState, TEventAdder> : Attribute
    where TEventAdder : IEvDbEventAdder
{
    public EvDbViewAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}