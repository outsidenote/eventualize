// Ignore Spelling: InMemory

using EvDb.Core;
using System.Collections.Immutable;

namespace EvDb.Adapters.Store.InMemory;

public record InMemoryUnit
{
    public required ImmutableList<EvDbEvent> Events { get; init; }
    public required ImmutableList<EvDbMessage> Messages { get; init; }
    public required ImmutableDictionary<EvDbViewAddress, EvDbStoredSnapshotData> Snapshots { get; init; }
}
