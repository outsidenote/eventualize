using System.Text.RegularExpressions;

namespace Core.Event;

public record StoredEvent<EventDataType>(string EventType, DateTime CapturedAt, string CapturedBy, EventDataType Data, DateTime StoredAt)
    : CapturedEvent<EventDataType>(EventType, CapturedAt, CapturedBy, Data);

