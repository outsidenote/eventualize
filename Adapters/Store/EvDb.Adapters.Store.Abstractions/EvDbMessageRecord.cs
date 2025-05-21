using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("MessageType: {MessageType} ,EventType:{EventType}, Channel:{Channel} Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord
{
    /// <summary>
    /// Unique identifier of the message
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// The address of the stream that the message produced from
    /// </summary>
    public string RootAddress { get; init; }
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
    public byte[] Payload { get; init; }
    /// <summary>
    /// The user that captured the event that produced the message
    /// </summary>
    public string CapturedBy { get; init; }
    /// <summary>
    /// The date and time that the event that produced the message was captured
    /// </summary>
    public DateTimeOffset CapturedAt { get; init; }
    /// <summary>
    /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
    /// The value will be null if the Trace is null when persisting the record or before persistent.
    /// </summary>
    public byte[]? TelemetryContext { get; init; }

    #region static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    public static implicit operator EvDbMessageRecord(EvDbMessage e)
    {
        var result = new EvDbMessageRecord
        {
            Id = Guid.NewGuid(), // TODO: GuidV7
            RootAddress = e.StreamCursor.RootAddress,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Channel = e.Channel,
            MessageType = e.MessageType,
            SerializeType = e.SerializeType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt,
            TelemetryContext = e.TelemetryContext
        };
        return result;
    }

    #endregion //  static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    #region GetMetadata

    /// <summary>
    /// Get the metadata of the message.
    /// </summary>
    /// <returns></returns>
    public IEvDbMessageMeta GetMetadata()
    {
        EvDbStreamCursor cursor = new EvDbStreamCursor(RootAddress, StreamId, Offset);
        var result = new EvDbMessageMeta(cursor,
                                         EventType,
                                         MessageType,
                                         Channel,
                                         CapturedAt,
                                         CapturedBy)
        {
            TelemetryContext = TelemetryContext
        };
        return result;
    }

    #region readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    private readonly record struct EvDbMessageMeta(EvDbStreamCursor StreamCursor,
                                                  string EventType,
                                                  string MessageType,
                                                  EvDbChannelName Channel,
                                                  DateTimeOffset CapturedAt,
                                                  string CapturedBy) : IEvDbMessageMeta
    {
        /// <summary>
        /// Json format of the Trace (Open Telemetry) propagated context at the persistent time.
        /// The value will be null if the Trace is null when persisting the record or before persistent.
        /// </summary>
        public byte[]? TelemetryContext { get; init; }
    }

    #endregion //  readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    #endregion //  GetMetadata
}
