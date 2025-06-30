using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.Processing;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Warning, """
                    No sinks registered for EvDb messages sink processor (Id= `{id}`).
                    """)]
    public static partial void LogSinkInMissing(this ILogger logger, string id);

    [LoggerMessage(LogLevel.Information, """
                    EvDb messages sink processor start listening.
                    """)]
    public static partial void LogStartListening(this ILogger logger, [LogProperties] SinkBag info);
}
