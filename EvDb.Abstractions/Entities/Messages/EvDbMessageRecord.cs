using System.Diagnostics;

namespace EvDb.Core.Adapters;

/// <summary>
/// Raw message record that is stored in the storage.
/// Can be cast to `EvDbMessage`.
/// </summary>
[DebuggerDisplay("ID: {Id}, MessageType: {MessageType} ,EventType:{EventType}, Channel:{Channel} Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord
{
    /// <summary>
    /// Unique identifier of the message
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// The address of the stream that the message produced from
    /// </summary>
    public string StreamType { get; init; }
    /// <summary>
    /// The identifier of the stream instance
    /// </summary>
    public string StreamId { get; init; }
    /// <summary>
    /// The offset of the event that produced the message
    /// </summary>
    public long Offset { get; init; }

    /// <summary>
    /// The type of the event that produced the message 
    /// </summary>
    public string EventType { get; init; }

    /// <summary>
    /// Represents a outbox's message tagging into semantic channel name
    /// </summary>
    public string Channel { get; init; }

    /// <summary>
    /// The type of the message
    /// </summary>
    public string MessageType { get; init; }

    /// <summary>
    /// The type of the serialization used to serialize the message's payload
    /// </summary>
    public string SerializeType { get; init; }

    /// <summary>
    /// The payload of the message
    /// </summary>
    public EvDbMessagePayloadName Payload { get; init; }

    /// <summary>
    /// The user that captured the event that produced the message
    /// </summary>
    public string CapturedBy { get; init; }

    /// <summary>
    /// The date and time that the event that produced the message was captured
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

    #region static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    public static implicit operator EvDbMessageRecord(EvDbMessage m)
    {
        var result = new EvDbMessageRecord
        {
            Id = m.Id, // TODO: GuidV7
            StreamType = m.StreamCursor.StreamType,
            StreamId = m.StreamCursor.StreamId,
            Offset = m.StreamCursor.Offset,
            EventType = m.EventType,
            Channel = m.Channel,
            MessageType = m.MessageType,
            SerializeType = m.SerializeType,
            Payload = m.Payload,
            CapturedBy = m.CapturedBy,
            CapturedAt = m.CapturedAt,
            TraceParent = m.TraceParent
        };
        return result;
    }

    public static implicit operator EvDbMessage(EvDbMessageRecord m)
    {
        var cursor = new EvDbStreamCursor(m.StreamType, m.StreamId, m.Offset);
        var result = new EvDbMessage
        {
            Id = m.Id,
            StreamCursor = cursor,
            EventType = m.EventType,
            Channel = m.Channel,
            MessageType = m.MessageType,
            SerializeType = m.SerializeType,
            Payload = m.Payload,
            CapturedBy = m.CapturedBy,
            CapturedAt = m.CapturedAt,
            TraceParent = m.TraceParent,
            StoredAt = m.StoredAt
        };
        return result;
    }

    #endregion //  static implicit operator EvDbMessageRecord(EvDbMessage m) ...

    #region GetMetadata

    /// <summary>
    /// Get the metadata of the message.
    /// </summary>
    /// <returns></returns>
    public IEvDbMessageMeta GetMetadata()
    {
        EvDbStreamCursor cursor = new EvDbStreamCursor(StreamType, StreamId, Offset);
        var result = new EvDbMessageMeta(Id,
                                         cursor,
                                         EventType,
                                         MessageType,
                                         Channel,
                                         CapturedAt,
                                         StoredAt,
                                         CapturedBy)
        {
            TraceParent = TraceParent
        };
        return result;
    }

    #region readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    private readonly record struct EvDbMessageMeta(
                                                  Guid Id,
                                                  EvDbStreamCursor StreamCursor,
                                                  EvDbEventTypeName EventType,
                                                  EvDbMessageTypeName MessageType,
                                                  EvDbChannelName Channel,
                                                  DateTimeOffset CapturedAt,
                                                  DateTimeOffset? StoredAt,
                                                  string CapturedBy) : IEvDbMessageMeta
    {
        /// <summary>
        /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
        /// The value will be null if the Trace is null when persisting the record or before persistent.
        /// </summary>
        public EvDbOtelTraceParent TraceParent { get; init; }
    }

    #endregion //  readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    #endregion //  GetMetadata

    #region GetAddress

    /// <summary>
    /// Get the address of the message.
    /// </summary>
    public EvDbStreamAddress GetAddress() => new EvDbStreamAddress(StreamType, StreamId);

    #endregion //  GetAddress

    #region GetAddress

    /// <summary>
    /// Get the message's cursor.
    /// </summary>
    public EvDbStreamAddress GetCursor() => new EvDbStreamCursor(StreamType, StreamId, Offset);

    #endregion //  GetAddress

    #region ToString

    /// <summary>
    /// Get the unique fields as string (root_address:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{StreamType}:{StreamId}:{Offset:000_000_000_000}";

    #endregion //  ToString
}
