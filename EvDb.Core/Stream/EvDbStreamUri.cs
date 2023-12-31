using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbStreamUri(string Domain, string StreamType, string StreamId) : EvDbStreamBaseUri(Domain, StreamType)
{
    public EvDbStreamUri(EvDbStreamBaseUri baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.StreamType, streamId)
    {
    }

    public override string ToString()
    {
        return $"{Domain}/{StreamType}/{StreamId}";
    }
}