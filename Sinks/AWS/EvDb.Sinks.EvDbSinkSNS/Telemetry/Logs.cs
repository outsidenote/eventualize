using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.EvDbSinkSNS;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb sink SNS: Published to target '{target}', MessageId:{messageId} | EvDb: Message Id `{evDbMessageId} | Status:{httpStatusCode}`.
                    """)]
    public static partial void LogPublished(this ILogger logger, EvDbSinkTarget target, Guid evDbMessageId, string messageId, string httpStatusCode);

    [LoggerMessage(LogLevel.Debug, """
                    SNS topic [{topicName}] created successfully.
                    """)]
    public static partial void LogSNSTopicExists(this ILogger logger,
                                                  string topicName);
}
