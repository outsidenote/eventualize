using Generator.Equals;

using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

/// <summary>
/// The address of a view. 
/// Built of the root address, stream id and view name.
/// </summary>
/// <param name="StreamType"></param>
/// <param name="StreamId"></param>
/// <param name="ViewName"></param>
[Equatable]
public readonly partial record struct EvDbViewAddress(string StreamType, string StreamId, string ViewName)
{
    public EvDbViewAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.StreamType, streamAddress.StreamId, viewName) { }

    #region IsEquals, ==, !=


    private bool IsEquals(EvDbStreamTypeName streamType)
    {
        if (this.StreamType != streamType)
            return false;

        return true;
    }

    private bool IsEquals(EvDbStreamAddress address)
    {
        if (this.StreamType != address.StreamType)
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

    public static bool operator ==(EvDbViewAddress viewAddress, EvDbStreamTypeName streamAddress)
    {
        return viewAddress.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbViewAddress viewAddress, EvDbStreamTypeName streamAddress)
    {
        return !viewAddress.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbViewAddress instance)
    {
        return new EvDbStreamAddress(instance.StreamType, instance.StreamId);
    }

    public static implicit operator EvDbStreamTypeName(EvDbViewAddress instance)
    {
        return instance.StreamType;
    }

    public static implicit operator EvDbViewBasicAddress(EvDbViewAddress instance)
    {
        return new EvDbViewBasicAddress(instance.StreamType, instance.ViewName);
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
                            .Add(TAG_ROOT_ADDRESS, StreamType)
                            .Add(TAG_STREAM_ID, StreamId)
                            .Add(TAG_VIEW_NAME, ViewName);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{StreamType}:{StreamId}:{ViewName}";
    }

    #endregion // ToString
}
