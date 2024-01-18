using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

// TODO [bnaya 2024-01-03] use IEvDbEventPayload instead of string EventType & Data
// TODO: [bnaya 2021-12-27] make it a struct with casting to EvDbStoredEvent
[Equatable]
[DebuggerDisplay("{EventType}")]
public partial record EvDbEventMeta(string EventType,
                                       [property: IgnoreEquality] DateTime CapturedAt,
                                       string CapturedBy) : IEvDbEventMeta;
[Equatable]
[DebuggerDisplay("{EventType}: {Data}")]
public partial record EvDbEvent(string eventType,
                                       [property: IgnoreEquality] DateTime capturedAt,
                                       string capturedBy,
                                       string Data) :
                                            EvDbEventMeta(eventType, capturedAt, capturedBy),
                                            IEvDbEvent
{
    T IEvDbEvent.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Data, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }

    //T IEvDbEvent.GetData<T>(JsonTypeInfo<T> context)
    //{
    //    var json = JsonSerializer.Deserialize(Data, context) ?? throw new InvalidCastException(typeof(T).Name);
    //    return json;
    //}
}
