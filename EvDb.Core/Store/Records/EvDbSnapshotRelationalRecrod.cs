namespace EvDb.Core;

// TODO [bnaya 2023-12-27] Rewrite it as a struct with internal field of EvDbSnapshotCursor
public record EvDbeSnapshotRelationalRecrod(string SerializedState, string Domain, string EntityType, string EntityId, string AggregateType, long Offset)
    : EvDbSnapshotCursor(Domain, EntityType, EntityId, AggregateType, Offset)
{
    public EvDbSnapshotCursor ToCursor => new EvDbSnapshotCursor(Domain, EntityType, EntityId, AggregateType, Offset);
}