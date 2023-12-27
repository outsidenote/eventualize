using Generator.Equals;

namespace Eventualize.Core.Abstractions;

[Equatable]
public partial record EventualizeStreamUri(string Domain, string StreamType, string StreamId) : EventualizeStreamBaseUri(Domain, StreamType)
{
    public EventualizeStreamUri(EventualizeStreamBaseUri baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.StreamType, streamId)
    {
    }

    public override string ToString()
    {
        return $"{Domain}/{StreamType}/{StreamId}";
    }
}