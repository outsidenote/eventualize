using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("{EventType}: {Payload}")]
public partial record struct EvDbEvent(EvDbEventTypeName EventType,
                                [property: IgnoreEquality] DateTimeOffset CapturedAt,
                                string CapturedBy,
                                EvDbStreamCursor StreamCursor,
                                EvDbEventPayloadName Payload) :
                                            IEvDbEventConverter,
                                            IEvDbEventMeta
{
    public static readonly EvDbEvent Empty = new EvDbEvent();

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
