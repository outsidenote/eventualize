using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("[{Topic} [{StreamCursor.Offset}]:{MessageType}] driven from [{EventType}]")]
public partial record struct EvDbMessage(
                                string EventType,
                                string Topic,
                                string MessageType,
                                [property: IgnoreEquality] DateTimeOffset CapturedAt,
                                string CapturedBy,
                                EvDbStreamCursor StreamCursor,
                                byte[] Payload) :
                                            IEvDbEventConverter,
                                            IEvDbTopicMeta
{
    public static readonly EvDbEvent Empty = new EvDbEvent();

    T IEvDbEventConverter.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Payload, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }
}
