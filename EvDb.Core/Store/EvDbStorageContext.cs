using System.Diagnostics;
using System.Text;

namespace EvDb.Core;

/// <summary>
/// Via the context you can differentiate the naming of the table prefix
/// </summary>
[DebuggerDisplay("📦{DatabaseName} 🧱{Schema} 🌀{Environment} > {Prefix}")]
public record EvDbStorageContext
{
    private readonly EvDbStorageContextData _data;

    #region Ctor

    /// <summary>
    /// Create instance
    /// </summary>
    /// <param name="databaseName">SerializerType of the database</param>
    /// <param name="prefix">Table prefix</param>
    /// <param name="environment">The environment (dev, prod, qa)</param>
    /// <param name="schema">the database schema</param>
    public EvDbStorageContext(EvDbDatabaseName databaseName,
                              Env? environment = null,
                              EvDbShardName? prefix = null,
                              EvDbSchemaName? schema = null) : this(new EvDbStorageContextData
                              {
                                  DatabaseName = databaseName,
                                  Environment = environment,
                                  Prefix = prefix,
                                  Schema = schema
                              })
    {
    }
    /// <summary>
    /// Create instance
    /// </summary>
    public EvDbStorageContext(EvDbStorageContextData data)
    {
        _data = data;
        var builder = new StringBuilder(200);
        var environment = data.Environment;
        if (!string.IsNullOrEmpty(environment))
            builder.Append($"{environment}_");

        var prefix = data.Prefix;
        if (!string.IsNullOrEmpty(prefix?.Value))
            builder.Append($"{prefix}_");

        ShortId = builder.ToString();

        var schema = data.Schema;
        if (!string.IsNullOrEmpty(schema?.Value))
            builder.Insert(0, $"{schema}.");

        Id = builder.ToString();
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
                        EvDbDatabaseName databaseName,
                        EvDbShardName? prefix = null,
                        string environmentOrKey = "ASPNETCORE_ENVIRONMENT",
                        EvDbSchemaName? schema = null)
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
    public EvDbDatabaseName DatabaseName => _data.DatabaseName;

    #endregion //  DatabaseName

    #region Environment

    /// <summary>
    /// Gets the environment (prod, qa, dev).
    /// </summary>
    public Env? Environment => _data.Environment;

    #endregion //  Environment

    #region Prefix

    /// <summary>
    /// Gets a prefix for the tables names.
    /// </summary>
    public EvDbShardName? Prefix => _data.Prefix;

    #endregion //  Prefix

    #region Schema

    /// <summary>
    /// Gets the table's schema.
    /// </summary>
    public EvDbSchemaName? Schema => _data.Schema;

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

    // public static implicit operator EvDbStorageContext(string prefix) => EvDbStorageContext.CreateWithEnvironment(prefix);

    public static implicit operator EvDbStorageContextData(EvDbStorageContext context)
    {
        return new EvDbStorageContextData
        {
            DatabaseName = context.DatabaseName,
            Environment = context.Environment,
            Prefix = context.Prefix,
            Schema = context.Schema
        };
    }

    public static implicit operator EvDbStorageContext(EvDbStorageContextData context)
    {
        return new EvDbStorageContext(context);
    }

    #endregion // Cast overloads 

}
