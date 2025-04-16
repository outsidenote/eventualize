using Generator.Equals;
using System.Diagnostics;
using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

/// <summary>
/// Identify the stream address, a unique instance of a stream
/// </summary>
/// <param name="Domain"></param>
/// <param name="Partition">Representation of a stream partition under the domain, like a User</param>
/// <param name="StreamId">The instance of a stream entity like { User: 'Joe' }</param>
[Equatable]
[DebuggerDisplay("{Domain}:{Partition}:{StreamId}")]
public readonly partial record struct EvDbStreamAddress(EvDbDomainName Domain, EvDbPartitionName Partition, string StreamId)
{
    public EvDbStreamAddress(EvDbPartitionAddress baseAddress, string streamId)
        : this(baseAddress.Domain, baseAddress.Partition, streamId)
    {
    }

    #region IsEquals, ==, !=


    private bool IsEquals(EvDbPartitionAddress partitionAddress)
    {
        if (this.Domain != partitionAddress.Domain)
            return false;
        if (this.Partition != partitionAddress.Partition)
            return false;

        return true;
    }

    public static bool operator ==(EvDbStreamAddress streamAddress, EvDbPartitionAddress partitionAddress)
    {
        return streamAddress.IsEquals(partitionAddress);
    }

    public static bool operator !=(EvDbStreamAddress streamAddress, EvDbPartitionAddress partitionAddress)
    {
        return !streamAddress.IsEquals(partitionAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbPartitionAddress(EvDbStreamAddress instance)
    {
        return new EvDbPartitionAddress(instance.Domain, instance.Partition);
    }


    #endregion // Casting Overloads

    #region ToOtelTagsToOtelTags

    /// <summary>
    /// Converts to open telemetry tags.
    /// </summary>
    /// <returns></returns>
    public OtelTags ToOtelTagsToOtelTags()
    {
        var tags = OtelTags.Empty
                            .Add(TAG_DOMAIN, Domain)
                            .Add(TAG_PARTITION, Partition)
                            .Add(TAG_STREAM_ID, StreamId);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{Domain}:{Partition}:{StreamId}";
    }

    #endregion // ToString
}