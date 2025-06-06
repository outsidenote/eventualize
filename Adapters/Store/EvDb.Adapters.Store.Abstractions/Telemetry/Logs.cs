﻿using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace EvDb.Core.Adapters;

public static partial class Logs
{
    [LoggerMessage(LogLevel.Trace, "Storage Query [{method}]: {query}")]
    public static partial void LogQuery(this ILogger logger, string query, [CallerMemberName] string? method = null);
}
