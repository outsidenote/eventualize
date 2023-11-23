namespace Core.Event;
using System.Text.Json;

public record CapturedEvent<EventDataType>(string EventType, DateTime CapturedAt, string CapturedBy, EventDataType Data)
{
    public string EventType = EventUtils.ValidateNonEmptyString(EventType);
    public string CapturedBy = EventUtils.ValidateNonEmptyString(CapturedBy);

    public string? SerializeData()
    {
        if (Data == null) return null;
        return JsonSerializer.Serialize(Data, Data.GetType());
    }

    public static EventDataType? DeserializeData(string? serializedData)
    {
        if (serializedData == null) return default;
        return JsonSerializer.Deserialize<EventDataType>(serializedData);
    }
}

