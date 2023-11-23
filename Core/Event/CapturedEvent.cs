namespace Core.Event;
using System.Text.Json;

public record CapturedEvent<EventDataType>(string EventType, DateTime CapturedAt, string CapturedBy, EventDataType Data)
{
    public string EventType = EventUtils.ValidateNonEmptyString(EventType);
    public string CapturedBy = EventUtils.ValidateNonEmptyString(CapturedBy);

    public string SerializeEventData(object eventData)
    {
        return JsonSerializer.Serialize(eventData, eventData.GetType());
    }
}

