namespace EvDb.Core;

public record StorageContext
{
    public static StorageContext Default { get; } = new StorageContext();
    public static StorageContext CreateWithEnvironment(string prefix = "_eventualize_")
    {
        Env env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ");
        string id = string.IsNullOrEmpty(env)
                            ? prefix
                            : $"_{prefix}_{env}_";
        return new StorageContext
        {
            Id = id,
        };
    }
    public static StorageContext CreateUnique(bool withEnvironment = true, string prefix = "_eventualize_")
    {
        Env env = withEnvironment
            ? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT ") ?? string.Empty
            : string.Empty;
        Guid guid = Guid.NewGuid();
        string unique = string.IsNullOrEmpty(env)
                            ? $"{prefix}{guid:N}_"
                            : $"{prefix}{env}_{guid:N}_";
        return new StorageContext
        {
            Id = unique,
        };
    }

    private StorageContext()
    {
    }

    /// <summary>
    /// Gets the context identifier.
    /// </summary>
    public string Id { get; init; } = string.Empty;


    public override string ToString() => Id;

    #region Cast overloads

    //public static implicit operator StorageContext(string id) => new StorageContext { Id = id };

    public static implicit operator string(StorageContext context) => context.ToString();

    #endregion // Cast overloads 

}