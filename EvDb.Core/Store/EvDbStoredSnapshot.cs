using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;


[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot(
            long Offset,
            string State)
{
    public EvDbStoredSnapshot() : this(-1, string.Empty) { }

    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot();
}