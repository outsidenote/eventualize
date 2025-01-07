using Generator.Equals;
using System.Diagnostics;

namespace EvDb.Core;

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot(
            long Offset,
            byte[] State): IEvDbStoredSnapshot
{
}

[Equatable]
[DebuggerDisplay("{State}, Offset:{Offset}")]
public readonly partial record struct EvDbStoredSnapshot<TState>(
            long Offset,
            TState State): IEvDbStoredSnapshot
{
}
