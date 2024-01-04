namespace EvDb.Core;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public class EvDbEventTypeAttribute<T> : Attribute
{
    public EvDbEventTypeAttribute(string eventType)
    {
        EventType = eventType;
    }

    public string EventType { get; }
}
