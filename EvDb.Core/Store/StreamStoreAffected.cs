using System.Collections.Immutable;

namespace EvDb.Core;

// TODO: [bnaya 2024-12-29] Add the views

/// <summary>
/// Indicate how many events and messages ware affected.
/// </summary>
/// <param name="Events">Stream table events</param>
/// <param name="Messages">Outbox messages</param>
public readonly record struct StreamStoreAffected(int Events, IImmutableDictionary<EvDbShardName, int> Messages)
{
    public static readonly StreamStoreAffected Empty = new StreamStoreAffected(0, ImmutableDictionary<EvDbShardName, int>.Empty);
}
