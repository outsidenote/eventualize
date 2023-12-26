using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public static class EventualizeEventFactory
{
    public static IEventualizeEvent Create<T>(string eventType,
                                       DateTime capturedAt,
                                       string capturedBy,
                                       T data,
                                       JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EventualizeEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json);
        return result;
    }

    public static IEventualizeEvent Create<T>(string eventType,
                                       DateTime capturedAt,
                                       string capturedBy,
                                       T data,
                                       JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EventualizeEvent(
                                                    eventType,
                                                    capturedAt,
                                                    capturedBy,
                                                    json);
        return result;
    }
}
