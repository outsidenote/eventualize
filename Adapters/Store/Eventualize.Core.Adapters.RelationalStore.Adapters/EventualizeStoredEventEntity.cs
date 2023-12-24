using Eventualize.Core.Abstractions.Stream;

// TODO: [bnaya 2023-12-20] default timeout

namespace Eventualize.Core.Adapters;

public struct EventualizeStoredEventEntity
{
    public string EventType { get; init; }
    public DateTime CapturedAt { get; init; }
    public string CapturedBy { get; init; }
    public string JsonData { get; init; }
    public DateTime StoredAt { get; init; }
    public string Domain { get; init; }
    public string StreamType { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }

    public static implicit operator EventualizeStoredEvent(EventualizeStoredEventEntity entity)
    {
        var srm = new EventualizeStreamUri(
                                            entity.Domain,
                                            entity.StreamType,
                                            entity.StreamId);
        return new EventualizeStoredEvent(
                    entity.EventType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    entity.JsonData,
                    entity.StoredAt,
                    srm,
                    entity.Offset);
    }
}
