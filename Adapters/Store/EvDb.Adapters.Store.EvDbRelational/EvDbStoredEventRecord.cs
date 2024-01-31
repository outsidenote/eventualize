// TODO: [bnaya 2023-12-20] default timeout

namespace EvDb.Core.Adapters;

internal struct EvDbStoredEventRecord
{
    public string EventType { get; init; }
    public DateTimeOffset CapturedAt { get; init; }
    public string CapturedBy { get; init; }
    public string Data { get; init; }
    public DateTimeOffset StoredAt { get; init; }
    public string Domain { get; init; }
    public string Partition { get; init; }
    public string StreamId { get; init; }
    public long Offset { get; init; }

    public static implicit operator EvDbEvent(EvDbStoredEventRecord entity)
    {
        EvDbStreamCursor StreamCursor = new(
                                            entity.Domain,
                                            entity.Partition,
                                            entity.StreamId,
                                            entity.Offset);
        return new EvDbEvent(
                    entity.EventType,
                    entity.CapturedAt,
                    entity.CapturedBy,
                    StreamCursor,
                    entity.Data);
    }
}
