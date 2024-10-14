using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("MessageType: {MessageType} ,EventType:{EventType}, Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbMessageRecord
{
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }
    public string EventType { get; init; }
    public string Topic { get; init; }
    public string MessageType { get; init; }
    public byte[] Payload { get; init; }
    public string CapturedBy { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    public static implicit operator EvDbMessage(EvDbMessageRecord entity)
    {
        EvDbStreamCursor StreamCursor = new(
                                            entity.Domain,
                                            entity.Partition,
                                            entity.StreamId,
                                            entity.Offset);
        return new EvDbMessage(
                    entity.EventType,
                    entity.Topic,
                    entity.MessageType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    StreamCursor,
                    entity.Payload);
    }

    public static implicit operator EvDbMessageRecord(EvDbMessage e)
    {
        return new EvDbMessageRecord
        {
            Domain = e.StreamCursor.Domain,
            Partition = e.StreamCursor.Partition,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            Topic = e.Topic,
            MessageType = e.MessageType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt
        };
    }
}
