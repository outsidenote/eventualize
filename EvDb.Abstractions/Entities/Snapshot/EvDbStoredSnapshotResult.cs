using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

// TODO: [bnaya 2025-01-23] rename to EvDbStoredSnapshotResult

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}, Stored At:{StoredAt}")]
public partial record EvDbStoredSnapshotResult(
            long Offset,
            DateTimeOffset? StoredAt,
            byte[] State) : EvDbStoredSnapshotResultBase(Offset, StoredAt), IEvDbStoredSnapshot
{
    public EvDbStoredSnapshotResult() : this(0, null, Array.Empty<byte>()) { }

    public static readonly EvDbStoredSnapshotResult Empty = new EvDbStoredSnapshotResult(0, null, Array.Empty<byte>());
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}, Stored At:{StoredAt}")]
public partial record EvDbStoredSnapshotResult<TState>(
            long Offset,
            DateTimeOffset? StoredAt,
            TState State) : EvDbStoredSnapshotResultBase(Offset, StoredAt), IEvDbStoredSnapshot
{
#pragma warning disable CS8604 // Possible null reference argument.
    public EvDbStoredSnapshotResult() : this(0, null, default) { }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
    public static readonly EvDbStoredSnapshotResult<TState> Empty = new EvDbStoredSnapshotResult<TState>(0, null, default);
#pragma warning restore CS8604 // Possible null reference argument.
}
