using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("MessageType: {MessageType} ,EventType:{EventType}, Channel:{Channel} Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord
{
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }
    public string EventType { get; init; }
    public string Channel { get; init; }
    public string? TraceId { get; init; }
    public string? SpanId { get; init; }
    public string MessageType { get; init; }
    public byte[] Payload { get; init; }
    public string CapturedBy { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    public static implicit operator EvDbMessageRecord(EvDbMessage e)
    {
        Activity? activity = Activity.Current;
        var result = new EvDbMessageRecord
        {
            Domain = e.StreamCursor.Domain,
            Partition = e.StreamCursor.Partition,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Channel = e.Channel,
            MessageType = e.MessageType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt,
            SpanId = activity?.SpanId.ToHexString(),
            TraceId = activity?.TraceId.ToHexString()
        };
        return result;
    }
}
