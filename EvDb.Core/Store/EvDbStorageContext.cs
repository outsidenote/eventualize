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

    public static EvDbStorageContext CreateWithEnvironment(string prefix = "", string environmentKey = "ASPNETCORE_ENVIRONMENT")
    {
        Env env = Environment.GetEnvironmentVariable(environmentKey);

        return new EvDbStorageContext(prefix, env);
    }

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id { get; }

    public override string ToString() => Id;

    #region Cast overloads

    public static implicit operator string(EvDbStorageContext context) => context.ToString();

    #endregion // Cast overloads 

}