using Generator.Equals;

namespace Eventualize.Core.Adapters;

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EventualizeAdapterQueryTemplates
{
    /// <summary>
    /// Get last snapshot sequence identifier.
    /// </summary>
    public string GetLastSnapshotSequenceId { get; init; }
    /// <summary>
    /// Get latest snapshot.
    /// </summary>
    public string TryGetSnapshot { get; init; }
    /// <summary>
    /// Get events.
    /// </summary>
    public string GetEvents { get; init; }
    /// <summary>
    /// Save.
    /// </summary>
    public string Save { get; init; }
    /// <summary>
    /// Save snapshot.
    /// </summary>
    public string SaveSnapshot { get; init; }
}
