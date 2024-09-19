using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core.Adapters;

public static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, "Affected topic's messages = {count}")]
    public static partial void LogAffectedMessages(this ILogger logger, long count);
}
