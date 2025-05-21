using Generator.Equals;

using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

[Equatable]
public readonly partial record struct EvDbViewBasicAddress(string StreamType, string ViewName)
{
    public EvDbViewBasicAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.StreamType, viewName) { }


    public EvDbViewBasicAddress(EvDbViewAddress viewAddress)
        : this(viewAddress.StreamType, viewAddress.ViewName) { }

    #region ToOtelTagsToOtelTags

    /// <summary>
    /// Converts to open telemetry tags.
    /// </summary>
    /// <returns></returns>
    public OtelTags ToOtelTagsToOtelTags()
    {
        OtelTags tags = OtelTags.Empty
                            .Add(TAG_ROOT_ADDRESS, StreamType)
                            .Add(TAG_VIEW_NAME, ViewName);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        if (string.IsNullOrWhiteSpace(ViewName))
            return StreamType;
        return $"{StreamType}:{ViewName}";
    }

    #endregion // ToString
}
