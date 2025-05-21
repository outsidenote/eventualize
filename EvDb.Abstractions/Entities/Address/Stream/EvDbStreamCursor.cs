using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Specific stream location.
/// </summary>
[DebuggerDisplay("Offset:{Offset}")]
[Equatable]
public readonly partial record struct EvDbStreamCursor
{
    public EvDbStreamCursor(EvDbStreamTypeName streamType, string streamId, long offset = 0)
    {
        StreamType = streamType;
        StreamId = streamId;
        Offset = offset;
    }

    public EvDbStreamCursor(EvDbStreamAddress streamId, long offset = 0)
        : this(streamId.StreamType, streamId.StreamId, offset) { }

    public string StreamType { get; }
    public string StreamId { get; }
    public long Offset { get; }

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

    public static bool operator ==(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    public static bool operator ==(EvDbStreamCursor cursor, EvDbStreamTypeName streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbStreamTypeName streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStreamCursor instance)
    {
        return new EvDbStreamAddress(instance.StreamType, instance.StreamId);
    }

    public static implicit operator EvDbStreamTypeName(EvDbStreamCursor instance)
    {
        return instance.StreamType;
    }


    #endregion // Casting Overloads

    #region ToString

    /// <summary>
    /// Get the unique fields as string (root_address:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{StreamType}:{StreamId}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (root_address:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToFilterString() => $"{StreamType}:{StreamId}:";

    #endregion //  ToString
}
