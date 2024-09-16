using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("{OutboxType}:{PayloadType}: {Payload}")]
public partial record struct EvDbOutboxEntity(
                                string EventType,
                                string OutboxType,
                                [property: IgnoreEquality] DateTimeOffset CapturedAt,
                                string CapturedBy,
                                EvDbStreamCursor StreamCursor,
                                string Payload) :
                                            IEvDbEventConverter,
                                            IEvDbOutboxMeta
{
    public static readonly EvDbEvent Empty = new EvDbEvent();

    T IEvDbEventConverter.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Payload, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }
}
