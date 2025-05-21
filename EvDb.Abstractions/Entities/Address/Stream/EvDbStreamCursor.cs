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
    public EvDbStreamCursor(EvDbRootAddressName rootAddress, string streamId, long offset = 0)
    {
        RootAddress = rootAddress;
        StreamId = streamId;
        Offset = offset;
    }

    public EvDbStreamCursor(EvDbStreamAddress streamId, long offset = 0)
        : this(streamId.RootAddress, streamId.StreamId, offset) { }

    public string RootAddress { get; }
    public string StreamId { get; }
    public long Offset { get; }

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

    public static bool operator ==(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    public static bool operator ==(EvDbStreamCursor cursor, EvDbRootAddressName streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbRootAddressName streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStreamCursor instance)
    {
        return new EvDbStreamAddress(instance.RootAddress, instance.StreamId);
    }

    public static implicit operator EvDbRootAddressName(EvDbStreamCursor instance)
    {
        return instance.RootAddress;
    }


    #endregion // Casting Overloads

    #region ToString

    /// <summary>
    /// Get the unique fields as string (root_address:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{RootAddress}:{StreamId}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (root_address:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToFilterString() => $"{RootAddress}:{StreamId}:";

    #endregion //  ToString
}
