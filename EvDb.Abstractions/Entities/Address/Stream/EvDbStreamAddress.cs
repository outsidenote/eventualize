using Generator.Equals;
using System.Diagnostics;
using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

/// <summary>
/// Identify the stream address, a unique instance of a stream
/// </summary>
/// <param name="RootAddress"></param>
/// <param name="StreamId">The instance of a stream entity like { User: 'Joe' }</param>
[Equatable]
[DebuggerDisplay("{RootAddress}:{StreamId}")]
public readonly partial record struct EvDbStreamAddress(EvDbRootAddressName RootAddress, string StreamId)
{
    #region IsEquals, ==, !=


    private bool IsEquals(EvDbRootAddressName rootAddress)
    {
        if (this.RootAddress != rootAddress)
            return false;

        return true;
    }

    public static bool operator ==(EvDbStreamAddress streamAddress, EvDbRootAddressName rootAddress)
    {
        return streamAddress.IsEquals(rootAddress);
    }

    public static bool operator !=(EvDbStreamAddress streamAddress, EvDbRootAddressName rootAddress)
    {
        return !streamAddress.IsEquals(rootAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbRootAddressName(EvDbStreamAddress instance)
    {
        return instance.RootAddress;
    }


    #endregion // Casting Overloads

    #region ToParameters

    /// <summary>
    /// Converts to parameters (string representation).
    /// </summary>
    /// <returns></returns>
    public Parameters ToParameters() => new Parameters(RootAddress.Value,  StreamId);

    #endregion //  ToParameters

    #region ToOtelTagsToOtelTags

    /// <summary>
    /// Converts to open telemetry tags.
    /// </summary>
    /// <returns></returns>
    public OtelTags ToOtelTagsToOtelTags()
    {
        var tags = OtelTags.Empty
                            .Add(TAG_ROOT_ADDRESS, RootAddress.Value)
                            .Add(TAG_STREAM_ID, StreamId);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{RootAddress}:{StreamId}";
    }

    #endregion // ToString

    public readonly record struct Parameters(string RootAddress, string StreamId);
}