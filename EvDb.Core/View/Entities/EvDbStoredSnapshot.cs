using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot(
            long Offset,
            byte[] State): IEvDbStoredSnapshot
{
    public static readonly EvDbStoredSnapshot Empty = new EvDbStoredSnapshot(0, Array.Empty<byte>());
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot<TState>(
            long Offset,
            TState State): IEvDbStoredSnapshot
{
    public static readonly EvDbStoredSnapshot<TState> Empty = new EvDbStoredSnapshot<TState>();
}
