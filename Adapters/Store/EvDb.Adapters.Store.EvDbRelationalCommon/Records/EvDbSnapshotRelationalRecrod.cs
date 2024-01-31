namespace EvDb.Core;

// TODO [bnaya 2023-12-27] Rewrite it as a struct with internal field of EvDbSnapshotCursor
public record EvDbeSnapshotRelationalRecrod(string SerializedState, string Domain, string Partition, string StreamId, string AggregateType, long Offset)
    : EvDbSnapshotCursor(Domain, Partition, StreamId, AggregateType, Offset)
{
    public EvDbSnapshotCursor ToCursor => new EvDbSnapshotCursor(Domain, Partition, StreamId, AggregateType, Offset);
}