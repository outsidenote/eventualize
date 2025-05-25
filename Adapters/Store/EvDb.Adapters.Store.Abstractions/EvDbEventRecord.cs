using System.Diagnostics;

namespace EvDb.Core.Adapters;


[DebuggerDisplay("EventType:{EventType}, Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbEventRecord
{
    /// <summary>
    /// Unique identifier of the event
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// The address of the stream  
    /// </summary>
    public string StreamType { get; init; }
    /// <summary>
    /// The identifier of the stream instance
    /// </summary>
    public string StreamId { get; init; }
    /// <summary>
    /// The offset of the event 
    /// </summary>
    public long Offset { get; init; }
    /// <summary>
    /// The type of the event  
    /// </summary>
    public string EventType { get; init; }
    /// <summary>
    /// The payload of the event
    /// </summary>
    public EvDbEventPayloadName Payload { get; init; }
    /// <summary>
    /// The user that captured the event 
    /// </summary>
    public string CapturedBy { get; init; }
    /// <summary>
    /// The date and time that the event 
    /// </summary>
    public DateTimeOffset CapturedAt { get; init; }
    /// <summary>
    /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
    /// The value will be null if the Trace is null when persisting the record or before persistent.
    /// </summary>
    public EvDbTelemetryContextName TelemetryContext { get; init; }

    #region Casting Overloads

    public static implicit operator EvDbEvent(EvDbEventRecord entity)
    {
        EvDbStreamCursor StreamCursor = new(
                                            entity.StreamType,
                                            entity.StreamId,
                                            entity.Offset);
        return new EvDbEvent(
                    entity.EventType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    StreamCursor,
                    entity.Payload)
        {
            TelemetryContext = entity.TelemetryContext
        };
    }

    public static implicit operator EvDbEventRecord(EvDbEvent e)
    {
        return new EvDbEventRecord
        {
            Id = Guid.NewGuid(), // TODO: GuidV7
            StreamType = e.StreamCursor.StreamType,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt,
            TelemetryContext = e.TelemetryContext
        };
    }

    #endregion //  Casting Overloads
}
