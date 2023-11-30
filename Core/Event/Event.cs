namespace Core.Event;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Event : IEquatable<Event>
{
    public readonly string EventType;
    public readonly DateTime CapturedAt;
    public readonly string CapturedBy;
    public readonly string JsonData;
    public readonly DateTime? StoredAt;
    public Event(string eventType, DateTime capturedAt, string capturedBy, string jsonData, DateTime? storedAt)
    {
        EventType = eventType;
        CapturedAt = capturedAt;
        CapturedBy = capturedBy;
        JsonData = jsonData;
        StoredAt = storedAt;
    }
    public Event(Event pendingEvent, DateTime storedAt)
    {
        EventType = pendingEvent.EventType;
        CapturedAt = pendingEvent.CapturedAt;
        CapturedBy = pendingEvent.CapturedBy;
        JsonData = pendingEvent.JsonData;
        StoredAt = storedAt;
    }

    public bool Equals(Event? other)
    {
        if (other == null)
            return false;

        var capturedAtSkew = (CapturedAt - other.CapturedAt).Duration();
        return
            EventType == other.EventType &&
            CapturedBy == other.CapturedBy &&
            JsonData == other.JsonData &&
            capturedAtSkew < TimeSpan.FromSeconds(0.5);
    }
};

