using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbSnapshotUri(string Domain, string StreamType, string StreamId, string AggregateType) : EvDbStreamUri(Domain, StreamType, StreamId)
{
    public EvDbSnapshotUri(EvDbStreamUri streamUri, string aggregateType)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, aggregateType) { }
    public EvDbSnapshotUri(EvDbAggregate aggregate)
        : this(aggregate.StreamUri, aggregate.AggregateType) { }

    public override string ToString()
    {
        return $"{Domain}/{StreamType}/{StreamId}/{AggregateType}";
    }
}
