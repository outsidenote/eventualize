using Generator.Equals;

using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

[Equatable]
public readonly partial record struct EvDbViewBasicAddress(string Domain, string Partition, string ViewName)
{
    public EvDbViewBasicAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.Domain, streamAddress.Partition, viewName) { }

    public EvDbViewBasicAddress(EvDbPartitionAddress streamAddress, string viewName)
        : this(streamAddress.Domain, streamAddress.Partition, viewName) { }

    public EvDbViewBasicAddress(EvDbViewAddress viewAddress)
        : this(viewAddress.Domain, viewAddress.Partition, viewAddress.ViewName) { }

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
                            .Add(TAG_VIEW_NAME, ViewName);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        if(string.IsNullOrWhiteSpace(ViewName))
            return $"{Domain}:{Partition}";
        return $"{Domain}:{Partition}:{ViewName}";
    }

    #endregion // ToString
}
