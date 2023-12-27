using Generator.Equals;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

// TODO: [bnaya 2021-12-27] make it a struct with casting to EventualizeStoredEvent
[Equatable]
public partial record EventualizeEvent(string EventType,
                                       [property: IgnoreEquality] DateTime CapturedAt,
                                       string CapturedBy,
                                       string Data) : 
                                            IEventualizeEvent
{
    T IEventualizeEvent.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Data, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }

    T IEventualizeEvent.GetData<T>(JsonTypeInfo<T> context)
    {
        var json = JsonSerializer.Deserialize(Data, context) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }
}
