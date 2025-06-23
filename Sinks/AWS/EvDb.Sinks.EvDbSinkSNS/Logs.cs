using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.EvDbSinkSNS;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb sink SNS: Publishing message to target '{target}', Message Id `{messageId}`.
                    """)]
    public static partial void LogPublish(this ILogger logger, EvDbSinkTarget target, Guid messageId);
}
