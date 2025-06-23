using EvDb.Sinks.EvDbSinkSNS;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryCoreExtensions
{
    public static TracerProviderBuilder AddEvDbInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelTrace.Name);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkSNSMeters.Default.Name);
        return builder;
    }
}

