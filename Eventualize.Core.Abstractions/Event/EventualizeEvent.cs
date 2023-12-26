using Generator.Equals;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Eventualize.Core;

public partial record EventualizeEvent(string EventType,
                                       [property: IgnoreEquality] DateTime CapturedAt,
                                       string CapturedBy,
                                       string Data) : 
                                            IEventualizeEvent, 
                                            IEqualityComparer<EventualizeEvent>
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


    bool IEqualityComparer<EventualizeEvent>.Equals(EventualizeEvent? x, EventualizeEvent? y)
    {
        return x?.Data == y?.Data &&
                x.EventType == y.EventType &&
                x.CapturedBy == y.CapturedBy;
    }

    int IEqualityComparer<EventualizeEvent>.GetHashCode(EventualizeEvent obj)
    {
        return Data.GetHashCode() ^ EventType.GetHashCode() ^ CapturedBy.GetHashCode();
    }
}
