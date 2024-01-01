using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbSnapshotId(string Domain, string EntityType, string EntityId, string AggregateType) : EvDbStreamId(Domain, EntityType, EntityId)
{
    public EvDbSnapshotId(EvDbStreamId streamId, string aggregateType)
        : this(streamId.Domain, streamId.EntityType, streamId.EntityId, aggregateType) { }
    public EvDbSnapshotId(EvDbAggregate aggregate)
        : this(aggregate.StreamId, aggregate.AggregateType) { }

    public override string ToString()
    {
        return $"{Domain}/{EntityType}/{EntityId}/{AggregateType}";
    }
}
