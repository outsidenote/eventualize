namespace EvDb.Core;

public readonly record struct TypedStorageOptions
{
    public static readonly TypedStorageOptions Default = new TypedStorageOptions
    {
        EvDbConnectionStringOrConfigurationKey = "EvDbSqlServerConnection",
        ContextConnectionStringOrConfigurationKey = "EvDbSqlServerConnection"
    };

    public string EvDbConnectionStringOrConfigurationKey { get; init; }
    public string ContextConnectionStringOrConfigurationKey { get; init; }
    public int? CommandTimeout { get; init; }
}

