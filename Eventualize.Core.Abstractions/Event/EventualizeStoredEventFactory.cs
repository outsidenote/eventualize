using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public static class EventualizeStoredEventFactory
{
    public static IEventualizeStoredEvent Create<T>(string eventType,
                                        DateTime capturedAt,
                                        string capturedBy,
                                        T data,
                                        DateTime storedAt,
                                        EventualizeStreamCursor streamCursor,
                                        JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EventualizeStoredEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json,
                                                    storedAt,
                                                    streamCursor);
        return result;
    }

    public static IEventualizeStoredEvent Create<T>(string eventType,
                                        DateTime capturedAt,
                                        string capturedBy,
                                        T data,
                                        DateTime storedAt,
                                        EventualizeStreamCursor streamCursor,
                                        JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EventualizeStoredEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json,
                                                    storedAt,
                                                    streamCursor);
        return result;
    }

    public static IEventualizeStoredEvent Create(IEventualizeEvent e,
                                        EventualizeStreamCursor streamCursor)
    {
        EventualizeEvent evt = (EventualizeEvent)e;
        var result = new EventualizeStoredEvent(e.EventType,
                                    e.CapturedAt,
                                    e.CapturedBy,
                                    evt.Data,
                                    DateTime.UtcNow,
                                    streamCursor);
        return result;
    }
}
