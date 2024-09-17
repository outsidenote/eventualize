using Generator.Equals;

namespace EvDb.Core.Adapters;

// TODO: bnaya 2024-09-17 split it into stream & snapshot

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EvDbAdapterQueryTemplates
{
    /// <summary>
    /// Get latest snapshot.
    /// </summary>
    public string GetSnapshot { get; init; }
    /// <summary>
    /// Get events.
    /// </summary>
    public string GetEvents { get; init; }
    /// <summary>
    /// Save Events.
    /// </summary>
    public string SaveEvents { get; init; }
    /// <summary>
    /// Save into the outbox table.
    /// </summary>
    public string SaveToOutbox { get; init; }
    /// <summary>
    /// SaveEvents snapshot.
    /// </summary>
    public string SaveSnapshot { get; init; }
}
