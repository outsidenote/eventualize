using Generator.Equals;

namespace EvDb.Core;

/// <summary>
/// Specific location of a snapshot, root-address:view_name:offset.
/// </summary>
/// <param name="StreamType"></param>
/// <param name="StreamId"></param>
/// <param name="ViewName"></param>
/// <param name="Offset"></param>
[Equatable]
public readonly partial record struct EvDbSnapshotCursor(string StreamType, string StreamId, string ViewName, long Offset = 0)
{
    public static readonly EvDbSnapshotCursor Empty = new EvDbSnapshotCursor("N/A", "N/A", "N/A", 0);

    public EvDbSnapshotCursor(EvDbStreamAddress streamAddress, string viewName, long offset = 0)
        : this(streamAddress.StreamType, streamAddress.StreamId, viewName, offset) { }

    #region IsEquals, ==, !=

    private bool IsEquals(EvDbViewAddress viewAddress)
    {
        if (this.StreamType != viewAddress.StreamType)
            return false;
        if (this.StreamId != viewAddress.StreamId)
            return false;
        if (this.ViewName != viewAddress.ViewName)
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

    public static bool operator ==(EvDbSnapshotCursor cursor, EvDbStreamAddress viewAddress)
    {
        return cursor.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbSnapshotCursor cursor, EvDbStreamAddress viewAddress)
    {
        return !cursor.IsEquals(viewAddress);
    }

    public static bool operator ==(EvDbSnapshotCursor cursor, EvDbViewAddress viewAddress)
    {
        return cursor.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbSnapshotCursor cursor, EvDbViewAddress viewAddress)
    {
        return !cursor.IsEquals(viewAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbSnapshotCursor instance)
    {
        return new EvDbStreamAddress(instance.StreamType, instance.StreamId);
    }

    public static implicit operator EvDbViewAddress(EvDbSnapshotCursor instance)
    {
        return new EvDbViewAddress(instance.StreamType, instance.StreamId, instance.ViewName);
    }

    #endregion // Casting Overloads

    #region ToString

    /// <summary>
    /// Get the unique fields as string (root_address:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{StreamType}:{StreamId}:{ViewName}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (root_address:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToFilterString() => $"{StreamType}:{StreamId}:{ViewName}:";

    #endregion //  ToString
}
