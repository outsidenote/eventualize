using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[DebuggerDisplay("Offset:{Offset}")]
[Equatable]
public readonly partial record struct EvDbStreamCursor(string Domain, string Partition, string StreamId, long Offset = 0)
{
    public EvDbStreamCursor(EvDbStreamAddress streamId, long offset = 0)
        : this(streamId.Domain, streamId.Partition, streamId.StreamId, offset) { }
    public EvDbStreamCursor(EvDbPartitionAddress partition, string streamId, long offset = 0)
        : this(partition.Domain, partition.Partition, streamId, offset) { }

    #region IsEquals, ==, !=


    private bool IsEquals(EvDbPartitionAddress partitionAddress)
    {
        if (this.Domain != partitionAddress.Domain)
            return false;
        if (this.Partition != partitionAddress.Partition)
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

    public static bool operator ==(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbStreamAddress streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    public static bool operator ==(EvDbStreamCursor cursor, EvDbPartitionAddress streamAddress)
    {
        return cursor.IsEquals(streamAddress);
    }

    public static bool operator !=(EvDbStreamCursor cursor, EvDbPartitionAddress streamAddress)
    {
        return !cursor.IsEquals(streamAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStreamCursor instance)
    {
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbPartitionAddress(EvDbStreamCursor instance)
    {
        return new EvDbPartitionAddress(instance.Domain, instance.Partition);
    }


    #endregion // Casting Overloads

    public override string ToString() => $"{Domain}:{Partition}:{StreamId}:{Offset}";
}
