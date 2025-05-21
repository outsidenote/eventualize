using System.Diagnostics;

namespace MongoBenchmark;

// Sample record definitions, similar to your provided types.
[DebuggerDisplay("Offset:{Offset}")]
public readonly record struct EvDbStreamCursor(string RootAddress, string StreamId, long Offset = 0)
{
    /// <summary>
    /// Get the unique fields as string (domain:stream_id:offset).
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{RootAddress}:{StreamId}:{Offset:000_000_000_000}";

    /// <summary>
    /// Get the filter fields as string (domain:stream_id:).
    /// </summary>
    /// <returns></returns>
    public string ToNonOffsetString() => $"{RootAddress}:{StreamId}:";
}
