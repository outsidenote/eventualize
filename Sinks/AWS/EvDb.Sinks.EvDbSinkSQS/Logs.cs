using EvDb.Core;
using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.EvDbSinkSQS;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    EvDb sink SQS: Publishing message to target '{target}'.
                    """)]
    public static partial void LogPublish(this ILogger logger, EvDbSinkTarget target, [LogProperties] EvDbMessage message);
}
