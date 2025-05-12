using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("[{ShardName}, {Channel} [{StreamCursor.Offset}]:{MessageType}] driven from [{EventType}]")]
public partial record struct EvDbMessage(
                                string EventType,
                                EvDbChannelName Channel,
                                EvDbShardName ShardName,
                                string MessageType,
                                string SerializeType,
                                [property: IgnoreEquality] DateTimeOffset CapturedAt,
                                string CapturedBy,
                                EvDbStreamCursor StreamCursor,
                                byte[] Payload) :
                                            IEvDbEventConverter,
                                            IEvDbMessageMeta
{
    public static readonly EvDbEvent Empty = new EvDbEvent();

    [IgnoreEquality]
    string? IEvDbMessageMeta.TraceId => Activity.Current?.TraceId.ToHexString();
    [IgnoreEquality]
    string? IEvDbMessageMeta.SpanId => Activity.Current?.SpanId.ToHexString();

    T IEvDbEventConverter.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Payload, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }
}
