using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EvDb.Core;

internal static class Telemetry
{
    public const string TraceName = "EvDb";

    public static ActivitySource Trace { get; } = new ActivitySource(TraceName);

    public static IEvDbSysMeters SysMeters { get; } = new EvDbSysMeters();
}
