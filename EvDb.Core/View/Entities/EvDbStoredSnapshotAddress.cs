using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshotData(
            Guid Id,
            string Domain,
            string Partition,
            string StreamId,
            string ViewName,
            long Offset,
            byte[] State)
{
    public EvDbStoredSnapshotData(
            EvDbViewAddress address,
            long offset,
            byte[] state)
                : this(Guid.NewGuid(), address.Domain, address.Partition, address.StreamId, address.ViewName, offset, state)
    {
    }

    #region IsEquals, ==, !=

    private bool IsEquals(EvDbStoredSnapshot snapshot)
    {
        if (this.Offset != snapshot.Offset)
            return false;
        if (this.State != snapshot.State)
            return false;

        return true;
    }

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

    private bool IsEquals(EvDbSnapshotCursor cursor)
    {
        if (this.Domain != cursor.Domain)
            return false;
        if (this.Partition != cursor.Partition)
            return false;
        if (this.StreamId != cursor.StreamId)
            return false;
        if (this.ViewName != cursor.ViewName)
            return false;
        if (this.Offset != cursor.Offset)
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

    public static bool operator ==(EvDbStoredSnapshotData left, EvDbStoredSnapshot right)
    {
        return left.IsEquals(right);
    }

    public static bool operator !=(EvDbStoredSnapshotData left, EvDbStoredSnapshot right)
    {
        return !left.IsEquals(right);
    }

    public static bool operator ==(EvDbStoredSnapshotData left, EvDbSnapshotCursor right)
    {
        return left.IsEquals(right);
    }

    public static bool operator !=(EvDbStoredSnapshotData left, EvDbSnapshotCursor right)
    {
        return !left.IsEquals(right);
    }

    public static bool operator ==(EvDbStoredSnapshotData left, EvDbStreamAddress viewAddress)
    {
        return left.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbStoredSnapshotData left, EvDbStreamAddress viewAddress)
    {
        return !left.IsEquals(viewAddress);
    }

    public static bool operator ==(EvDbStoredSnapshotData left, EvDbViewAddress viewAddress)
    {
        return left.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbStoredSnapshotData left, EvDbViewAddress viewAddress)
    {
        return !left.IsEquals(viewAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStoredSnapshotData instance)
    {
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbViewAddress(EvDbStoredSnapshotData instance)
    {
        return new EvDbViewAddress(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName);
    }

    public static implicit operator EvDbStoredSnapshot(EvDbStoredSnapshotData instance)
    {
        return new EvDbStoredSnapshot(instance.Offset, instance.State);
    }

    public static implicit operator EvDbSnapshotCursor(EvDbStoredSnapshotData instance)
    {
        return new EvDbSnapshotCursor(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName, instance.Offset);
    }

    #endregion // Casting Overloads
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshotData<TState>(
            Guid Id,
            string Domain,
            string Partition,
            string StreamId,
            string ViewName,
            long Offset,
            TState State)
{
    public EvDbStoredSnapshotData(
            EvDbViewAddress address,
            long offset,
            TState state)
                : this(Guid.NewGuid(), address.Domain, address.Partition, address.StreamId, address.ViewName, offset, state)
    {
    }

    #region IsEquals, ==, !=

    private bool IsEquals(EvDbStoredSnapshot snapshot)
    {
        if (Offset != snapshot.Offset)
            return false;
        if (Equals(State, snapshot.State))
            return false;

        return true;
    }

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

    private bool IsEquals(EvDbSnapshotCursor cursor)
    {
        if (this.Domain != cursor.Domain)
            return false;
        if (this.Partition != cursor.Partition)
            return false;
        if (this.StreamId != cursor.StreamId)
            return false;
        if (this.ViewName != cursor.ViewName)
            return false;
        if (this.Offset != cursor.Offset)
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

    public static bool operator ==(EvDbStoredSnapshotData<TState> left, EvDbStoredSnapshot right)
    {
        return left.IsEquals(right);
    }

    public static bool operator !=(EvDbStoredSnapshotData<TState> left, EvDbStoredSnapshot right)
    {
        return !left.IsEquals(right);
    }

    public static bool operator ==(EvDbStoredSnapshotData<TState> left, EvDbSnapshotCursor right)
    {
        return left.IsEquals(right);
    }

    public static bool operator !=(EvDbStoredSnapshotData<TState> left, EvDbSnapshotCursor right)
    {
        return !left.IsEquals(right);
    }

    public static bool operator ==(EvDbStoredSnapshotData<TState> left, EvDbStreamAddress viewAddress)
    {
        return left.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbStoredSnapshotData<TState> left, EvDbStreamAddress viewAddress)
    {
        return !left.IsEquals(viewAddress);
    }

    public static bool operator ==(EvDbStoredSnapshotData<TState> left, EvDbViewAddress viewAddress)
    {
        return left.IsEquals(viewAddress);
    }

    public static bool operator !=(EvDbStoredSnapshotData<TState> left, EvDbViewAddress viewAddress)
    {
        return !left.IsEquals(viewAddress);
    }

    #endregion // IsEquals, ==, !=

    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStoredSnapshotData<TState> instance)
    {
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbViewAddress(EvDbStoredSnapshotData<TState> instance)
    {
        return new EvDbViewAddress(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName);
    }

    public static implicit operator EvDbSnapshotCursor(EvDbStoredSnapshotData<TState> instance)
    {
        return new EvDbSnapshotCursor(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName, instance.Offset);
    }

    #endregion // Casting Overloads
}
