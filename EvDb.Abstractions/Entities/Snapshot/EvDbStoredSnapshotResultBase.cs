using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("Offset:{Offset}")]
public partial record EvDbStoredSnapshotResultBase
{
    protected EvDbStoredSnapshotResultBase(long offset,
                                     DateTimeOffset? storedAt)
    {
        Offset = offset;
        StoredAt = storedAt;
    }

    public static readonly EvDbStoredSnapshotResultBase None = new (0, null);

    public long Offset { get; init; }
    public DateTimeOffset? StoredAt { get; init; }
}
