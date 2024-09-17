// TODO: [bnaya 2023-12-20] default timeout

using System.Diagnostics;

namespace EvDb.Core.Adapters;

[DebuggerDisplay("OutboxType: {OutboxType} ,EventType:{EventType}, Offset:{Offset}, StreamId:{StreamId}")]
public struct EvDbOutboxRecord
{
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }
    public string EventType { get; init; }
    public string OutboxType { get; init; }
    public string Payload { get; init; }
    public string CapturedBy { get; init; }
    public DateTimeOffset CapturedAt { get; init; }

    public static implicit operator EvDbOutboxEntity(EvDbOutboxRecord entity)
    {
        EvDbStreamCursor StreamCursor = new(
                                            entity.Domain,
                                            entity.Partition,
                                            entity.StreamId,
                                            entity.Offset);
        return new EvDbOutboxEntity(
                    entity.EventType,
                    entity.OutboxType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    StreamCursor,
                    entity.Payload);
    }

    public static implicit operator EvDbOutboxRecord(EvDbOutboxEntity e)
    {
        return new EvDbOutboxRecord
        {
            Domain = e.StreamCursor.Domain,
            Partition = e.StreamCursor.Partition,
            StreamId = e.StreamCursor.StreamId,
            Offset = e.StreamCursor.Offset,
            EventType = e.EventType,
            OutboxType = e.OutboxType,
            Payload = e.Payload,
            CapturedBy = e.CapturedBy,
            CapturedAt = e.CapturedAt
        };
    }
}
