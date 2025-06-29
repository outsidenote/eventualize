using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace EvDb.Core.Adapters;

public static partial class Logs
{
    [LoggerMessage(LogLevel.Trace, "Storage Query [{method}]: {query}")]
    public static partial void LogQuery(this ILogger logger, string query, [CallerMemberName] string? method = null);

    [LoggerMessage(LogLevel.Debug, "FetchedFromOutbox [{id}]: StreamType:{streamType}, StreamId:{streamId}, Offset:{offset}, EventType{eventType}, Channel:{channel}, Shard:{shard}")]
    public static partial void LogFetchedFromOutbox(this ILogger logger, Guid id, string streamType, string streamId, long offset, string eventType, string channel, string shard);
}
