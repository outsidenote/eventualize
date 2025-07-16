using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.EvDbSinkSQS;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb sink SQS: Published to target '{target}', MessageId:{messageId} | EvDb: Message Id `{evDbMessageId}` | Status:{httpStatusCode}.
                    """)]
    public static partial void LogPublished(this ILogger logger, EvDbSinkTarget target, Guid evDbMessageId, string messageId, string httpStatusCode);

}
