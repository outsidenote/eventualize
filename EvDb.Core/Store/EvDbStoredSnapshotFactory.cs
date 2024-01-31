using Generator.Equals;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;


[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public partial record EvDbStoredSnapshot(
            long Offset,
            string State)
{
    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot(-1, string.Empty);
}