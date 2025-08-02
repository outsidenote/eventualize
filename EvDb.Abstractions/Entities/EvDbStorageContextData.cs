using System.Diagnostics;

namespace EvDb.Core;

/// <summary>
/// Via the context you can differentiate the naming of the table prefix
/// </summary>
[DebuggerDisplay("📦{DatabaseName} 🧱{Schema} 🌀{Environment} > {Prefix}")]
public readonly record struct EvDbStorageContextData
{
    #region DatabaseName

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public EvDbDatabaseName DatabaseName { get; init; }

    #endregion //  DatabaseName

    #region Environment

    /// <summary>
    /// Gets the environment (prod, qa, dev).
    /// </summary>
    public Env? Environment { get; init; }

    #endregion //  Environment

    #region Prefix

    /// <summary>
    /// Gets a prefix for the tables names.
    /// </summary>
    public EvDbShardName? Prefix { get; init; }

    #endregion //  Prefix

    #region Schema

    /// <summary>
    /// Gets the table's schema.
    /// </summary>
    public EvDbSchemaName? Schema { get; init; }

    #endregion //  Schema
}
