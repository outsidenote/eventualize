using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public class EventualizeEventFactory
{
    public readonly string EventType;
    public readonly string CapturedBy;

    public EventualizeEventFactory(string eventType, string capturedBy)
    {
        EventType = eventType;
        CapturedBy = capturedBy;
    }
}

public class EventualizeEventFactory<T>(string eventType, string capturedBy) : EventualizeEventFactory(eventType, capturedBy), IEventualizeEventFactory<T>
{
    public IEventualizeEvent Create(DateTime capturedAt, T data, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EventualizeEvent(EventType, capturedAt, CapturedBy, json);
        return result;
    }
    public IEventualizeEvent Create(T data, JsonSerializerOptions? options = null)
    {
        return Create(DateTime.Now, data, options);
    }

    public IEventualizeEvent Create(DateTime capturedAt, T data, JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EventualizeEvent(EventType,
                                            capturedAt,
                                            CapturedBy,
                                            json);
        return result;
    }

    public IEventualizeEvent Create(T data, JsonTypeInfo<T> jsonType)
    {
        return Create(DateTime.Now, data, jsonType);
    }
}

