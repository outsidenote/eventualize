using Generator.Equals;

namespace EvDb.Core.Adapters;

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
    /// SaveEvents.
    /// </summary>
    public string SaveEvents { get; init; }
    /// <summary>
    /// SaveEvents snapshot.
    /// </summary>
    public string SaveSnapshot { get; init; }
}
