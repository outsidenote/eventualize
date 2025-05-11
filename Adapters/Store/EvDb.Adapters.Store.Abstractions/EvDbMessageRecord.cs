using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("MessageType: {MessageType} ,EventType:{EventType}, Channel:{Channel} Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord 
{
    public Guid Id { get; init; }
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }
    public string EventType { get; init; }
    public string Channel { get; init; }
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    public string MessageType { get; init; }
    public string SerializeType { get; init; }
    public byte[] Payload { get; init; }
    public string CapturedBy { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    #region static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    public static implicit operator EvDbMessageRecord(EvDbMessage e)
    {
        Activity? activity = Activity.Current;
        var result = new EvDbMessageRecord
        {
            Id = Guid.NewGuid(), // TODO: GuidV7
            Domain = e.StreamCursor.Domain,
            Partition = e.StreamCursor.Partition,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Channel = e.Channel,
            MessageType = e.MessageType,
            SerializeType = e.SerializeType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt,
            SpanId = activity?.SpanId.ToHexString(),
            TraceId = activity?.TraceId.ToHexString()
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
        EvDbStreamCursor cursor = new EvDbStreamCursor(Domain, Partition, StreamId, Offset);
        var result = new EvDbMessageMeta(cursor,
                                         EventType,
                                         MessageType,
                                         Channel,
                                         CapturedAt,
                                         CapturedBy);
        return result;
    }

    #region readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    private readonly record struct EvDbMessageMeta(EvDbStreamCursor StreamCursor,
                                                  string EventType,
                                                  string MessageType,
                                                  EvDbChannelName Channel,
                                                  DateTimeOffset CapturedAt,
                                                  string CapturedBy) : IEvDbMessageMeta;

    #endregion //  readonly record EvDbMessageMeta struct(...): IEvDbMessageMeta

    #endregion //  GetMetadata
}
