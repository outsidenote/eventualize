using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbViewAddress(string Domain, string Partition, string StreamId, string ViewName) : EvDbStreamAddress(Domain, Partition, StreamId)
{
    public EvDbViewAddress(EvDbStreamAddress streamId, string viewName)
        : this(streamId.Domain, streamId.Partition, streamId.StreamId, viewName) { }

    public EvDbViewAddress(IEvDbStreamStore aggregate, string viewName)
        : this(aggregate.StreamAddress, viewName) { }

    public override string ToString()
    {
        return $"{Domain}/{Partition}/{StreamId}/{ViewName}";
    }
}
