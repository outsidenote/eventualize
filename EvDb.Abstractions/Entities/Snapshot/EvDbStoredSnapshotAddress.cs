using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("Offset:{Offset}")]
public abstract partial record EvDbStoredSnapshotDataBase(
            Guid Id,
            string Domain,
            string Partition,
            string StreamId,
            string ViewName,
            long Offset,
            long StoreOffset)
{
    #region Casting Overloads

    public static implicit operator EvDbStreamAddress(EvDbStoredSnapshotDataBase instance)
    {
        return new EvDbStreamAddress(instance.Domain, instance.Partition, instance.StreamId);
    }

    public static implicit operator EvDbViewAddress(EvDbStoredSnapshotDataBase instance)
    {
        return new EvDbViewAddress(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName);
    }

    public static implicit operator EvDbSnapshotCursor(EvDbStoredSnapshotDataBase instance)
    {
        return new EvDbSnapshotCursor(instance.Domain, instance.Partition, instance.StreamId, instance.ViewName, instance.Offset);
    }

    #endregion // Casting Overloads

    #region IsEquals, ==, !=

    protected bool IsEquals(EvDbViewAddress viewAddress)
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

    protected bool IsEquals(EvDbSnapshotCursor cursor)
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

    protected bool IsEquals(EvDbStreamAddress address)
    {
        if (this.Domain != address.Domain)
            return false;
        if (this.Partition != address.Partition)
            return false;
        if (this.StreamId != address.StreamId)
            return false;

        return true;
    }

    #endregion // IsEquals, ==, !=
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public partial record EvDbStoredSnapshotData(
            Guid Id,
            string Domain,
            string Partition,
            string StreamId,
            string ViewName,
            long Offset,
            long StoreOffset,
            byte[] State) : EvDbStoredSnapshotDataBase(Id, Domain, Partition, StreamId, ViewName, Offset, StoreOffset)
{
    public EvDbStoredSnapshotData(
            EvDbViewAddress address,
            long offset,
            long storeOffset,
            byte[] state)
                : this(Guid.NewGuid(), address.Domain, address.Partition, address.StreamId, address.ViewName, offset, storeOffset, state)
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

    public static implicit operator EvDbStoredSnapshot(EvDbStoredSnapshotData instance)
    {
        return new EvDbStoredSnapshot(instance.Offset, instance.State);
    }

    #endregion // Casting Overloads
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public partial record EvDbStoredSnapshotData<TState>(
            Guid Id,
            string Domain,
            string Partition,
            string StreamId,
            string ViewName,
            long Offset,
            long StoreOffset,
            TState State) : EvDbStoredSnapshotDataBase(Id, Domain, Partition, StreamId, ViewName, Offset, StoreOffset)
{
    public EvDbStoredSnapshotData(
            EvDbViewAddress address,
            long offset,
            long storeOffset,
            TState state)
                : this(Guid.NewGuid(), address.Domain, address.Partition, address.StreamId, address.ViewName, offset, storeOffset, state)
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
