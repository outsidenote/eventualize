using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

[Obsolete("use EvDbEventFactory")]
public class EvDbEventFactoryLegacy
{
    public readonly string EventType;
    public readonly string CapturedBy;

    public EvDbEventFactoryLegacy(string eventType, string capturedBy)
    {
        EventType = eventType;
        CapturedBy = capturedBy;
    }

    public IEvDbEvent Create<T>(DateTime capturedAt, T data, JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(EventType, capturedAt, CapturedBy, json);
        return result;
    }

    public IEvDbEvent Create<T>(T data, JsonSerializerOptions? options = null)
    {
        return Create(DateTime.Now, data, options);
    }

    public IEvDbEvent Create<T>(DateTime capturedAt, T data, JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EvDbEvent(EventType,
                                            capturedAt,
                                            CapturedBy,
                                            json);
        return result;
    }

    public IEvDbEvent Create<T>(T data, JsonTypeInfo<T> jsonType)
    {
        return Create(DateTime.Now, data, jsonType);
    }
}

[Obsolete("deprecated")]
public class EvDbEventFactoryLegacy<T>(string eventType, string capturedBy) : EvDbEventFactoryLegacy(eventType, capturedBy), IEvDbEventFactory<T>
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

