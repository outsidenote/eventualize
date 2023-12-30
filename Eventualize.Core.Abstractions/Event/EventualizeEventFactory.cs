using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public class EventualizeEventFactory<T>
{
    public readonly string EventType;
    public EventualizeEventFactory(string eventType)
    {
        EventType = eventType;
    }
    public IEventualizeEvent Create(DateTime capturedAt,
                                    string capturedBy,
                                    T data,
                                    JsonSerializerOptions? options = null)
    {
        var json = JsonSerializer.Serialize(data, options);
        var result = new EventualizeEvent(EventType,
                                            capturedAt,
                                            capturedBy,
                                            json);
        return result;
    }
    public IEventualizeEvent Create(string capturedBy,
                                    T data,
                                    JsonSerializerOptions? options = null)
    {
        return Create(DateTime.Now, capturedBy, data, options);
    }

    public IEventualizeEvent Create(DateTime capturedAt,
                                       string capturedBy,
                                       T data,
                                       JsonTypeInfo<T> jsonType)
    {
        var json = JsonSerializer.Serialize(data, jsonType);
        var result = new EventualizeEvent(EventType,
                                            capturedAt,
                                            capturedBy,
                                            json);
        return result;
    }

    public IEventualizeEvent Create(string capturedBy,
                                       T data,
                                       JsonTypeInfo<T> jsonType)
    {
        return Create(DateTime.Now, capturedBy, data, jsonType);
    }
}
