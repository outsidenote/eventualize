using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace EvDb.Core;

public static class TelemetryCoreExtensions
{
    public static TracerProviderBuilder AddEvDbInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(Telemetry.TraceName);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbInstrumentation(this MeterProviderBuilder builder,
                                                              EvDbMeters include = EvDbMeters.All)
    {
        if ((include & EvDbMeters.SystemCounters) != EvDbMeters.None)
            builder.AddMeter(EvDbSysMeters.MetricCounterName);
        if ((include & EvDbMeters.SystemDuration) != EvDbMeters.None)
            builder.AddMeter(EvDbSysMeters.MetricDurationName);
        return builder;
    }
}

