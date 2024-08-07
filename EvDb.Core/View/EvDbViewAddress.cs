using Generator.Equals;
using System.Net;

using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

[Equatable]
public readonly partial record struct EvDbViewAddress(string Domain, string Partition, string StreamId, string ViewName)
{
    public EvDbViewAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.Domain, streamAddress.Partition, streamAddress.StreamId, viewName) { }

    #region IsEquals, ==, !=


    private bool IsEquals(EvDbPartitionAddress partitionAddress)
    {
        if (this.Domain != partitionAddress.Domain)
            return false;
        if (this.Partition != partitionAddress.Partition)
            return false;

        return true;
    }

    private bool IsEquals(EvDbStreamAddress address)
    {
        if (this.Domain != address.Domain)
            return false;
        if (this.Partition != address.Partition)
            return false;
        if (this.StreamId != address.StreamId)
            return false;

        return true;
    }

    public static bool operator ==(EvDbViewAddress viewAddress, EvDbStreamAddress streamAddress)
    {
        return viewAddress.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbViewAddress viewAddress, EvDbStreamAddress streamAddress)
    {
        return !viewAddress.IsEquals(streamAddress);
    }

    public static bool operator ==(EvDbViewAddress viewAddress, EvDbPartitionAddress streamAddress)
    {
        return viewAddress.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbViewAddress viewAddress, EvDbPartitionAddress streamAddress)
    {
        return !viewAddress.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbViewAddress instance)
    {
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbPartitionAddress(EvDbViewAddress instance)
    {
        return new EvDbPartitionAddress(instance.Domain, instance.Partition);
    }

    public static implicit operator EvDbViewBasicAddress(EvDbViewAddress instance)
    {
        return new EvDbViewBasicAddress(instance.Domain, instance.Partition, instance.ViewName);
    }

    #endregion // Casting Overloads

    #region ToOtelTagsToOtelTags

    /// <summary>
    /// Converts to open telemetry tags.
    /// </summary>
    /// <returns></returns>
    public OtelTags ToOtelTagsToOtelTags()
    {
        OtelTags tags = OtelTags.Empty
                            .Add(TAG_DOMAIN, Domain)
                            .Add(TAG_PARTITION, Partition)
                            .Add(TAG_STREAM_ID, StreamId)
                            .Add(TAG_VIEW_NAME, ViewName);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{Domain}/{Partition}/{StreamId}/{ViewName}";
    }

    #endregion // ToString
}
