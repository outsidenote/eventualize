using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store;

public static partial class Logs
{
    [LoggerMessage(LogLevel.Trace, "Sharding [{DatabaseName}:{CollectionName}]: {Sharding}")]
    public static partial void LogSharding(this ILogger logger, string databaseName, string collectionName, string sharding);
}
