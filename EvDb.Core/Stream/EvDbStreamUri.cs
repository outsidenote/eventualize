using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public partial record EvDbStreamId(string Domain, string EntityType, string EntityId) 
    : EvDbStreamType(Domain, EntityType)
{
    public EvDbStreamId(EvDbStreamType baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.EntityType, streamId)
    {
    }

    public override string ToString()
    {
        return $"{Domain}/{EntityType}/{EntityId}";
    }
}