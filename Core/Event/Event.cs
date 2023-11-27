namespace Core.Event;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Event
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
};

