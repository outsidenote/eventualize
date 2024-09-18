using Generator.Equals;

namespace EvDb.Core.Adapters;

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EvDbSnapshotAdapterQueryTemplates
{
    /// <summary>
    /// Get latest snapshot.
    /// </summary>
    public string GetSnapshot { get; init; }
    /// <summary>
    /// SaveEvents snapshot.
    /// </summary>
    public string SaveSnapshot { get; init; }
}
