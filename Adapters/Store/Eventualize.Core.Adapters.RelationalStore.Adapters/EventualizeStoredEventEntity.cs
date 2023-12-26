// TODO: [bnaya 2023-12-20] default timeout

namespace Eventualize.Core.Adapters;

public struct EventualizeStoredEventEntity
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

    public static implicit operator EventualizeStoredEvent(EventualizeStoredEventEntity entity)
    {
        EventualizeStreamCursor StreamCursor = new(
                                            entity.Domain,
                                            entity.StreamType,
                                            entity.StreamId,
                                            entity.Offset);
        return new EventualizeStoredEvent(
                    entity.EventType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    entity.Data,
                    entity.StoredAt,
                    StreamCursor);
    }
}
