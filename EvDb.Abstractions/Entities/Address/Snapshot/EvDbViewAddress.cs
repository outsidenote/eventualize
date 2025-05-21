using Generator.Equals;

using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

/// <summary>
/// The address of a view. 
/// Built of the root address, stream id and view name.
/// </summary>
/// <param name="RootAddress"></param>
/// <param name="StreamId"></param>
/// <param name="ViewName"></param>
[Equatable]
public readonly partial record struct EvDbViewAddress(string RootAddress, string StreamId, string ViewName)
{
    public EvDbViewAddress(EvDbStreamAddress streamAddress, string viewName)
        : this(streamAddress.RootAddress, streamAddress.StreamId, viewName) { }

    #region IsEquals, ==, !=


    private bool IsEquals(EvDbRootAddressName rootAddress)
    {
        if (this.RootAddress != rootAddress)
            return false;

        return true;
    }

    private bool IsEquals(EvDbStreamAddress address)
    {
        if (this.RootAddress != address.RootAddress)
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

    public static bool operator ==(EvDbViewAddress viewAddress, EvDbRootAddressName streamAddress)
    {
        return viewAddress.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbViewAddress viewAddress, EvDbRootAddressName streamAddress)
    {
        return !viewAddress.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbViewAddress instance)
    {
        return new EvDbStreamAddress(instance.RootAddress, instance.StreamId);
    }

    public static implicit operator EvDbRootAddressName(EvDbViewAddress instance)
    {
        return instance.RootAddress;
    }

    public static implicit operator EvDbViewBasicAddress(EvDbViewAddress instance)
    {
        return new EvDbViewBasicAddress(instance.RootAddress, instance.ViewName);
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
                            .Add(TAG_ROOT_ADDRESS, RootAddress)
                            .Add(TAG_STREAM_ID, StreamId)
                            .Add(TAG_VIEW_NAME, ViewName);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{RootAddress}:{StreamId}:{ViewName}";
    }

    #endregion // ToString
}
