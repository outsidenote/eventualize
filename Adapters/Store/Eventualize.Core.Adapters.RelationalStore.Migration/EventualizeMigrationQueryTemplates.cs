using Generator.Equals;

namespace Eventualize.Core.Adapters;

/// <summary>
/// Queries permutation with a specific context
/// </summary>
[Equatable]
public readonly partial record struct EventualizeMigrationQueryTemplates
{
    /// <summary>
    /// Create environment query.
    /// </summary>
    public string CreateEnvironment { get; init; }
    /// <summary>
    /// Destroy environment query.
    /// </summary>
    public string DestroyEnvironment { get; init; }
}
