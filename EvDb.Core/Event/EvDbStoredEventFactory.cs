using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

public static class EvDbStoredEventFactory
{
    public static IEvDbStoredEvent Create<T>(string eventType,
                                        DateTime capturedAt,
                                        string capturedBy,
                                        T data,
                                        DateTime storedAt,
                                        EvDbStreamCursor streamCursor,
                                        JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EvDbStoredEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json,
                                                    storedAt,
                                                    streamCursor);
        return result;
    }

    public static IEvDbStoredEvent Create<T>(string eventType,
                                        DateTime capturedAt,
                                        string capturedBy,
                                        T data,
                                        DateTime storedAt,
                                        EvDbStreamCursor streamCursor,
                                        JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EvDbStoredEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json,
                                                    storedAt,
                                                    streamCursor);
        return result;
    }

    public static IEvDbStoredEvent Create(IEvDbEvent e,
                                        EvDbStreamCursor streamCursor)
    {
        EvDbEvent evt = (EvDbEvent)e;
        var result = new EvDbStoredEvent(e.EventType,
                                    e.CapturedAt,
                                    e.CapturedBy,
                                    evt.Data,
                                    DateTime.UtcNow,
                                    streamCursor);
        return result;
    }
}
