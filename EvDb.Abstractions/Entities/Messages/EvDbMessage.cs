using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("[{ShardName}, {Channel} [{StreamCursor.Offset}]:{MessageType}] driven from [{EventType}]")]
public partial record struct EvDbMessage(
                                Guid Id, 
                                EvDbEventTypeName EventType,
                                EvDbChannelName Channel,
                                EvDbShardName ShardName,
                                EvDbMessageTypeName MessageType,
                                string SerializeType,
                                [property: IgnoreEquality] DateTimeOffset CapturedAt,
                                string CapturedBy,
                                EvDbStreamCursor StreamCursor,
                                EvDbMessagePayloadName Payload) :
                                            IEvDbEventConverter//,
                                                               //IEvDbMessageMeta
{
    public static readonly EvDbMessage Empty = new EvDbMessage() { Id = Guid.Empty };

    public EvDbMessage(EvDbEventTypeName EventType,
                       EvDbChannelName Channel,
                       EvDbShardName ShardName,
                       EvDbMessageTypeName MessageType,
                       string SerializeType,
                       [property: IgnoreEquality] DateTimeOffset CapturedAt,
                       string CapturedBy,
                       EvDbStreamCursor StreamCursor,
                       EvDbMessagePayloadName Payload): this(Guid.NewGuid(),
                                                                EventType,
                                                                Channel,
                                                                ShardName,
                                                                MessageType,
                                                                SerializeType,
                                                                CapturedAt,
                                                                CapturedBy,
                                                                StreamCursor,
                                                                Payload)
    {
    }

    /// <summary>
    /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
    /// The value will be null if the Trace is null when persisting the record or before persistent.
    /// </summary>
    public EvDbTelemetryContextName TelemetryContext { get; init; }

    /// <summary>
    /// The time when it persist into the storage
    /// </summary>
    public DateTimeOffset? StoredAt { get; init; }

    T IEvDbEventConverter.GetData<T>(JsonSerializerOptions? options)
    {
        var json = JsonSerializer.Deserialize<T>(Payload, options) ?? throw new InvalidCastException(typeof(T).Name);
        return json;
    }
}
