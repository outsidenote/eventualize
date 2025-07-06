using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.EvDbSinkKafka;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb sink Kafka: Published to target '{target}', Key:{key} | EvDb: Message Id `{evDbMessageId} | Persistence Status:{persistenceStatus}, Offset: {offset}`.
                    """)]
    public static partial void LogPublished(this ILogger logger, EvDbSinkTarget target, Guid evDbMessageId, string key, string persistenceStatus, long offset);

    [LoggerMessage(LogLevel.Error, """
                    EvDb sink Kafka: Failed to publish to Kafka topic {target}.
                    """)]
    public static partial void LogPublishedError(this ILogger logger, EvDbSinkTarget target, Exception error);
}
