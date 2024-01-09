using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public static class EvDbEventFactory 
{
    public static IEvDbEvent Create<T>(
        T data, string capturedBy, JsonSerializerOptions? options)
        where T: IEvDbEventPayload
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbEvent(data.EventType, DateTime.UtcNow, capturedBy, json);
        return result;
    }

    public static IEvDbEvent Create<T>(
        T data, string capturedBy, JsonTypeInfo<T> jsonType)
        where T : IEvDbEventPayload
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EvDbEvent(data.EventType, DateTime.UtcNow, capturedBy, json);
        return result;
    }
}

