namespace Core.Event;
using System.Text.Json;
using System.Text.Json.Nodes;

public record Event(string EventType, DateTime CapturedAt, string CapturedBy, string JsonData, DateTime? StoredAt);

