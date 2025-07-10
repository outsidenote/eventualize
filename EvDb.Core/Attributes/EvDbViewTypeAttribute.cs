#pragma warning disable S2326 // Unused type parameters should be removed
namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class EvDbViewTypeAttribute
    <TState, TEventBundle> : Attribute
    where TEventBundle : IEvDbEventTypes
{
    public EvDbViewTypeAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}