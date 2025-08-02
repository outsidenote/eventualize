using System.Diagnostics;

namespace EvDb.Core.Adapters;

/// <summary>
/// Raw event record that is stored in the storage.
/// Can be cast to `EvDbEvent`.
/// </summary>
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
    /// The date and time that the event was captured 
    /// </summary>
    public DateTimeOffset CapturedAt { get; init; }
    /// <summary>
    /// The Trace Parent (Open Telemetry) propagated context at the persistent time.
    /// The value will be null if the Trace is null when persisting the record or before persistent.
    /// </summary>
    public EvDbOtelTraceParent TraceParent { get; init; }

    /// <summary>
    /// The time when it persist into the storage
    /// </summary>
    public DateTimeOffset? StoredAt { get; init; }

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
            TraceParent = entity.TraceParent,
            StoredAt = entity.StoredAt,
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
            TraceParent = e.TraceParent
        };
    }

    #endregion //  Casting Overloads
}
