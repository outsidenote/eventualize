using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;


[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot(
            long Offset,
            string State)
{
    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot(-1, string.Empty);
}
