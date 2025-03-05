using System.Diagnostics;

namespace MongoBenchmark;

// Sample record definitions, similar to your provided types.
[DebuggerDisplay("Offset:{Offset}")]
public readonly record struct EvDbStreamCursor(string Domain, string Partition, string StreamId, long Offset = 0)
{
    /// <summary>
    /// Get the unique fields as string (domain:partition:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Domain}:{Partition}:{StreamId}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (domain:partition:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToNonOffsetString() => $"{Domain}:{Partition}:{StreamId}:";
}
