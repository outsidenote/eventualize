namespace EvDb.Core;

public class EvDbStorageContext
{
    public EvDbStorageContext(string prefix, Env environment)
    {
        Id = (prefix, environment) switch
        {
            (null, Env e) when (string.IsNullOrEmpty(e)) => string.Empty,
            ("", Env e) when (string.IsNullOrEmpty(e)) => string.Empty,
            (string p, Env e) when (string.IsNullOrEmpty(e)) => $"{p}_",
            (null, Env e) => $"{e}_",
            ("", Env e) => $"{e}_",
            _ => $"{prefix}_{environment}_"
        };
    }

    /// <summary>
    /// The environment variable key represent the environment.
    /// If the key is not found, the value is used as the environment.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="environmentOrKey"></param>
    /// <returns></returns>
    public static EvDbStorageContext CreateWithEnvironment(string prefix = "", string environmentOrKey = "ASPNETCORE_ENVIRONMENT")
    {
        Env env = Environment.GetEnvironmentVariable(environmentOrKey) ?? environmentOrKey;

        return new EvDbStorageContext(prefix, env);
    }

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id { get; }

    public override string ToString() => Id;

    #region Cast overloads

    public static implicit operator string(EvDbStorageContext context) => context.ToString();

    public static implicit operator EvDbStorageContext(string prefix) => EvDbStorageContext.CreateWithEnvironment(prefix);

    #endregion // Cast overloads 
}