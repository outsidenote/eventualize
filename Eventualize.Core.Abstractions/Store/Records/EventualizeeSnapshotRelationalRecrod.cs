namespace Eventualize.Core;

// TODO [bnaya 2023-12-27] Rewrite it as a struct with internal field of EventualizeSnapshotCursor
public record EventualizeeSnapshotRelationalRecrod(string SerializedState, string Domain, string StreamType, string StreamId, string AggregateType, long Offset)
    : EventualizeSnapshotCursor(Domain, StreamType, StreamId, AggregateType, Offset)
{
    public EventualizeSnapshotCursor ToCursor => new EventualizeSnapshotCursor(Domain, StreamType, StreamId, AggregateType, Offset);
}