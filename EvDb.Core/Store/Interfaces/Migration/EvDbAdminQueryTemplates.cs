using Generator.Equals;

namespace EvDb.Core;

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EvDbAdminQueryTemplates
{
    /// <summary>
    /// Create environment query.
    /// </summary>
    public string[] CreateEnvironment { get; init; }
    /// <summary>
    /// Destroy environment query.
    /// </summary>
    public string DestroyEnvironment { get; init; }
}
