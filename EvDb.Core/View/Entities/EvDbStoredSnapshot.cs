using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

// TODO: [bnaya 2025-01-23] rename to EvDbStoredSnapshotResult

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public partial record EvDbStoredSnapshot(
            long Offset,
            byte[] State) : EvDbStoredSnapshotBase(Offset), IEvDbStoredSnapshot
{
    public EvDbStoredSnapshot(): this(0, Array.Empty<byte>()) { }  

    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot(0, Array.Empty<byte>());
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public partial record EvDbStoredSnapshot<TState>(
            long Offset,
            TState State) : EvDbStoredSnapshotBase(Offset), IEvDbStoredSnapshot
{
#pragma warning disable CS8604 // Possible null reference argument.
    public EvDbStoredSnapshot(): this(0, default) { }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
    public static readonly EvDbStoredSnapshot<TState> Empty = new EvDbStoredSnapshot<TState>(0, default);
#pragma warning restore CS8604 // Possible null reference argument.
}
