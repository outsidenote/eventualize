using System.Diagnostics;

namespace MongoBenchmark;

// Sample record definitions, similar to your provided types.
[DebuggerDisplay("Offset:{Offset}")]
public readonly record struct EvDbStreamCursor(string Domain, string Partition, string StreamId, long Offset = 0)
{
    public override string ToString() => $"{Domain}:{Partition}:{StreamId}:{Offset:0_000_000_000}";
}
