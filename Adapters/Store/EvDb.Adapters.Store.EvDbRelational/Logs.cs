using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core.Adapters;

public static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, "Affected outbox records = {records}")]
    public static partial void LogAffectedOutbox(this ILogger logger, long records);
}
