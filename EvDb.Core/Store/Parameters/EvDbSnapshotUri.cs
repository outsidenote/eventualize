using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbSnapshotId(string Domain, string Partition, string StreamId, string AggregateType) : EvDbStreamAddress(Domain, Partition, StreamId)
{
    public EvDbSnapshotId(EvDbStreamAddress streamId, string aggregateType)
        : this(streamId.Domain, streamId.Partition, streamId.StreamId, aggregateType) { }
    public EvDbSnapshotId(EvDbAggregate aggregate)
        : this(aggregate.StreamId, aggregate.AggregateType) { }

    public override string ToString()
    {
        return $"{Domain}/{Partition}/{StreamId}/{AggregateType}";
    }
}
