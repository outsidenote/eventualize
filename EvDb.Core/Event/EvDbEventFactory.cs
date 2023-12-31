using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public class EvDbEventFactory
{
    public readonly string EventType;
    public readonly string CapturedBy;

    public EvDbEventFactory(string eventType, string capturedBy)
    {
        EventType = eventType;
        CapturedBy = capturedBy;
    }
}

public class EvDbEventFactory<T>(string eventType, string capturedBy) : EvDbEventFactory(eventType, capturedBy), IEvDbEventFactory<T>
{
    public IEvDbEvent Create(DateTime capturedAt, T data, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(EventType, capturedAt, CapturedBy, json);
        return result;
    }
    public IEvDbEvent Create(T data, JsonSerializerOptions? options = null)
    {
        return Create(DateTime.Now, data, options);
    }

    public IEvDbEvent Create(DateTime capturedAt, T data, JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EvDbEvent(EventType,
                                            capturedAt,
                                            CapturedBy,
                                            json);
        return result;
    }

    public IEvDbEvent Create(T data, JsonTypeInfo<T> jsonType)
    {
        return Create(DateTime.Now, data, jsonType);
    }
}

