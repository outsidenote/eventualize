using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbViewAddress(string Domain, string Partition, string StreamId, string ViewName) : EvDbStreamAddress(Domain, Partition, StreamId)
{
    public EvDbViewAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.Domain, streamAddress.Partition, streamAddress.StreamId, viewName) { }

    public override string ToString()
    {
        return $"{Domain}/{Partition}/{StreamId}/{ViewName}";
    }
}
