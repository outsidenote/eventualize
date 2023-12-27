using Eventualize.Core.Abstractions;
using Generator.Equals;

namespace Eventualize.Core;

[Equatable]
public partial record EventualizeSnapshotUri(string Domain, string StreamType, string StreamId, string AggregateType) : EventualizeStreamUri(Domain, StreamType, StreamId)
{
    public EventualizeSnapshotUri(EventualizeStreamUri streamUri, string aggregateType)
        : this(streamUri.Domain, streamUri.StreamType, streamUri.StreamId, aggregateType) { }
    public EventualizeSnapshotUri(EventualizeAggregate aggregate)
        : this(aggregate.StreamUri, aggregate.AggregateType) { }

    public override string ToString()
    {
        return $"{Domain}/{StreamType}/{StreamId}/{AggregateType}";
    }
}
