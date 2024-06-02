using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core.Adapters;

internal static partial class Telemetry
{
    [LoggerMessage(LogLevel.Debug, "Storage Query [{method}]: {query}")]
    public static partial void LogQuery(this ILogger logger, string query, [CallerMemberName] string? method = null);
}
