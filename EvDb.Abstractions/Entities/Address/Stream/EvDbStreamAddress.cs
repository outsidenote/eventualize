using Generator.Equals;
using System.Diagnostics;
using static EvDb.Core.OtelConstants;

namespace EvDb.Core;

/// <summary>
/// Identify the stream address, a unique instance of a stream
/// </summary>
/// <param name="StreamType"></param>
/// <param name="StreamId">The instance of a stream entity like { User: 'Joe' }</param>
[Equatable]
[DebuggerDisplay("{StreamType}:{StreamId}")]
public readonly partial record struct EvDbStreamAddress(EvDbStreamTypeName StreamType, string StreamId)
{
    #region IsEquals, ==, !=


    private bool IsEquals(EvDbStreamTypeName streamType)
    {
        if (this.StreamType != streamType)
            return false;

        return true;
    }

    public static bool operator ==(EvDbStreamAddress streamAddress, EvDbStreamTypeName streamType)
    {
        return streamAddress.IsEquals(streamType);
    }

    public static bool operator !=(EvDbStreamAddress streamAddress, EvDbStreamTypeName streamType)
    {
        return !streamAddress.IsEquals(streamType);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamTypeName(EvDbStreamAddress instance)
    {
        return instance.StreamType;
    }


    #endregion // Casting Overloads

    #region ToParameters

    /// <summary>
    /// Converts to parameters (string representation).
    /// </summary>
    /// <returns></returns>
    public Parameters ToParameters() => new Parameters(StreamType.Value, StreamId);

    #endregion //  ToParameters

    #region ToOtelTagsToOtelTags

    /// <summary>
    /// Converts to open telemetry tags.
    /// </summary>
    /// <returns></returns>
    public OtelTags ToOtelTagsToOtelTags()
    {
        var tags = OtelTags.Empty
                            .Add(TAG_ROOT_ADDRESS, StreamType.Value)
                            .Add(TAG_STREAM_ID, StreamId);
        return tags;
    }

    #endregion //  ToOtelTagsToOtelTags

    #region ToString

    public override string ToString()
    {
        return $"{StreamType}:{StreamId}";
    }

    #endregion // ToString

    public readonly record struct Parameters(string StreamType, string StreamId);
}