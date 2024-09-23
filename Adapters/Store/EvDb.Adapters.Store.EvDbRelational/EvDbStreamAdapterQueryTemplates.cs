using Generator.Equals;

namespace EvDb.Core.Adapters;

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EvDbStreamAdapterQueryTemplates
{
    /// <summary>
    /// Get events.
    /// </summary>
    public string GetEvents { get; init; }
    /// <summary>
    /// Save Events.
    /// </summary>
    public string SaveEvents { get; init; }
    /// <summary>
    /// Save into the topic table.
    /// </summary>
    public string SaveToTopics { get; init; }
}
