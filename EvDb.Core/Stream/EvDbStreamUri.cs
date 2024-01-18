using Generator.Equals;

namespace EvDb.Core;

/// <summary>
/// Identify the stream address, a unique instance of a stream
/// </summary>
/// <param name="Domain"></param>
/// <param name="Partition">Representation of a stream partition under the domain, like a User</param>
/// <param name="StreamId">The instance of a stream entity like { User: 'Joe' }</param>
[Equatable]
public partial record EvDbStreamAddress(string Domain, string Partition, string StreamId)
    : EvDbPartitionAddress(Domain, Partition)
{
    public EvDbStreamAddress(EvDbPartitionAddress baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.Partition, streamId)
    {
    }

    public override string ToString()
    {
        return $"{Domain}/{Partition}/{StreamId}";
    }
}