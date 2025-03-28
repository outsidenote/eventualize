﻿using Generator.Equals;

namespace EvDb.Core;

[Equatable]
public readonly partial record struct EvDbSnapshotCursor(string Domain, string Partition, string StreamId, string ViewName, long Offset = 0)
{
    public static readonly EvDbSnapshotCursor Empty = new EvDbSnapshotCursor("N/A", "N/A", "N/A", "N/A", 0);

    public EvDbSnapshotCursor(EvDbStreamAddress streamAddress, string viewName, long offset = 0)
        : this(streamAddress.Domain, streamAddress.Partition, streamAddress.StreamId, viewName, offset) { }

    #region IsEquals, ==, !=

    private bool IsEquals(EvDbViewAddress viewAddress)
    {
        if (this.Domain != viewAddress.Domain)
            return false;
        if (this.Partition != viewAddress.Partition)
            return false;
        if (this.StreamId != viewAddress.StreamId)
            return false;
        if (this.ViewName != viewAddress.ViewName)
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
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbViewAddress(EvDbSnapshotCursor instance)
    {
        return new EvDbViewAddress(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName);
    }

    #endregion // Casting Overloads

    #region ToString

    /// <summary>
    /// Get the unique fields as string (domain:partition:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Domain}:{Partition}:{StreamId}:{ViewName}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (domain:partition:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToFilterString() => $"{Domain}:{Partition}:{StreamId}:{ViewName}:";

    #endregion //  ToString
}
