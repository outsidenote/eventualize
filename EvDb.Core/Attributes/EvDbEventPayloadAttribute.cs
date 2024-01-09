namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbEventPayloadAttribute : Attribute
{
    public EvDbEventPayloadAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}