// TODO: [bnaya 2023-12-20] default timeout

namespace EvDb.Core.Adapters;

public struct EvDbStoredEventEntity
{
    public string EventType { get; init; }
    public DateTime CapturedAt { get; init; }
    public string CapturedBy { get; init; }
    public string Data { get; init; }
    public DateTime StoredAt { get; init; }
    public string Domain { get; init; }
    public string StreamType { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }

    public static implicit operator EvDbStoredEvent(EvDbStoredEventEntity entity)
    {
        EvDbStreamCursor StreamCursor = new(
                                            entity.Domain,
                                            entity.StreamType,
                                            entity.StreamId,
                                            entity.Offset);
        return new EvDbStoredEvent(
                    entity.EventType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    entity.Data,
                    entity.StoredAt,
                    StreamCursor);
    }
}
