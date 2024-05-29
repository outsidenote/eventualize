using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
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
public static class TelemetryExtensions
{
    public static TracerProviderBuilder AddEvDbInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(Telemetry.TraceName);
        return builder;
    }
    public static MeterProviderBuilder AddEvDbInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSysMeters.MetricName);
        return builder;
    }
}

