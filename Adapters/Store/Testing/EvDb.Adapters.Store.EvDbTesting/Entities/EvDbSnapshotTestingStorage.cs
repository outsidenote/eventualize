// Ignore Spelling: Sql Testing

using EvDb.Adapters.Store.Testing;
using EvDb.Core;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using SNAPSHOTS = System.Collections.Immutable.IImmutableList<EvDb.Core.EvDbStoredSnapshotData>;

namespace Microsoft.Extensions.DependencyInjection;

public record EvDbSnapshotTestingStorage
{
    public IImmutableDictionary<EvDbViewAddress, SNAPSHOTS> Store { get; init; } = ImmutableDictionary<EvDbViewAddress, SNAPSHOTS>.Empty;
}