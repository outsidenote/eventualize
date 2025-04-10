using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

// TODO: [bnaya 2025-01-23] rename to EvDbStoredSnapshotResultBase

[Equatable]
[DebuggerDisplay("Offset:{Offset}")]
public partial record EvDbStoredSnapshotBase
{
    protected EvDbStoredSnapshotBase(long offset)
    {
        Offset = offset;
    }

    public static readonly EvDbStoredSnapshotBase None = new EvDbStoredSnapshotBase(0);

    public long Offset { get; init; }
}
