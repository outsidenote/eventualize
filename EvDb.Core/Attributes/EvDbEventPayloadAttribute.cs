namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Assembly, AllowMultiple = true)]
public class EvDbEventPayloadAttribute : Attribute
{
    public EvDbEventPayloadAttribute(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}