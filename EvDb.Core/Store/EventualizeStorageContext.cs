namespace EvDb.Core;

public record EvDbStorageContext
{
    public static EvDbStorageContext Default { get; } = new EvDbStorageContext();
    public static EvDbStorageContext CreateWithEnvironment(string prefix = "_eventualize_")
    {
        Env env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ");
        string id = string.IsNullOrEmpty(env)
                            ? prefix
                            : $"_{prefix}_{env}_";
        return new EvDbStorageContext
        {
            Id = id,
        };
    }
    public static EvDbStorageContext CreateUnique(bool withEnvironment = true, string prefix = "_eventualize_")
    {
        Env env = withEnvironment
            ? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ") ?? string.Empty
            : string.Empty;
        Guid guid = Guid.NewGuid();
        string unique = string.IsNullOrEmpty(env)
                            ? $"{prefix}{guid:N}_"
                            : $"{prefix}{env}_{guid:N}_";
        return new EvDbStorageContext
        {
            Id = unique,
        };
    }

    private EvDbStorageContext()
    {
    }

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;


    public override string ToString() => Id;

    #region Cast overloads

    //public static implicit operator StorageContext(string id) => new StorageContext { Id = id };

    public static implicit operator string(EvDbStorageContext context) => context.ToString();

    #endregion // Cast overloads 

}