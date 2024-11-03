using System.Diagnostics;
using System.Text;

namespace EvDb.Core;

/// <summary>
/// Via the context you can differentiate the naming of the table prefix
/// </summary>
[DebuggerDisplay("{databaseName}: {Id}")]
public record EvDbStorageContext
{
    #region Ctor

    /// <summary>
    /// Create instance
    /// </summary>
    /// <param name="databaseName">Name of the database</param>
    /// <param name="prefix">Table prefix</param>
    /// <param name="environment">The environment (dev, prod, qa)</param>
    /// <param name="schema">the database schema</param>
    public EvDbStorageContext(EvDbTableName databaseName,
                              Env? environment = null,
                              EvDbTableName? prefix = null,
                              EvDbTableName? schema = null)
    {
        var builder = new StringBuilder(200);
        if (!string.IsNullOrEmpty(environment))
            builder.Append($"{environment}_");

        if (!string.IsNullOrEmpty(prefix?.Value))
            builder.Append($"{prefix}_");

        ShortId = builder.ToString();

        if (!string.IsNullOrEmpty(schema?.Value))
            builder.Insert(0, $"{schema}.");

        Id = builder.ToString();

        DatabaseName = databaseName;
        Environment = environment;
        Prefix = prefix;
        Schema = schema;
    }

    #endregion //  Ctor

    #region CreateWithEnvironment

    /// <summary>
    /// The environment variable key represent the environment.
    /// If the key is not found, the value is used as the environment.
    /// </summary>
    /// <param name="databaseName">The database name</param>
    /// <param name="prefix">Table prefix</param>
    /// <param name="environmentOrKey">
    /// The environment (dev, prod, qa) or 
    /// environment key where the environment can be fetched
    /// for example `ASPNETCORE_ENVIRONMENT`
    /// if the environment key exist the environment data will be taken form it 
    /// otherwise it will take the `environmentOrKey` as is.
    /// </param>
    /// <param name="schema">the database schema</param>
    /// <returns></returns>
    public static EvDbStorageContext CreateWithEnvironment(
                        EvDbTableName databaseName,
                        EvDbTableName? prefix = null,
                        string environmentOrKey = "ASPNETCORE_ENVIRONMENT",
                        EvDbTableName? schema = null)
    {
        Env env = System.Environment.GetEnvironmentVariable(environmentOrKey) ?? environmentOrKey;

        return new EvDbStorageContext(databaseName, env, prefix, schema);
    }

    #endregion //  CreateWithEnvironment

    #region ShortId

    /// <summary>
    /// Gets the context identifier that don't includes the schema.
    /// </summary>
    public string ShortId { get; }

    #endregion //  ShortId

    #region Id

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id { get; }

    #endregion //  Id

    #region DatabaseName

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public EvDbTableName DatabaseName { get; }

    #endregion //  DatabaseName

    #region Environment

    /// <summary>
    /// Gets the environment (prod, qa, dev).
    /// </summary>
    public Env? Environment { get; }

    #endregion //  Environment

    #region Prefix

    /// <summary>
    /// Gets a prefix for the tables names.
    /// </summary>
    public EvDbTableName? Prefix { get; }

    #endregion //  Prefix

    #region Schema

    /// <summary>
    /// Gets the table's schema.
    /// </summary>
    public EvDbTableName? Schema { get; }

    #endregion //  Schema

    #region ToString

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Id;

    #endregion //  ToString

    #region Cast overloads

    public static implicit operator string(EvDbStorageContext context) => context.ToString();

    public static implicit operator EvDbStorageContext(string prefix) => EvDbStorageContext.CreateWithEnvironment(prefix);

    #endregion // Cast overloads 
}