using EvDb.Sinks.EvDbSinkSNS;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryCoreExtensions
{
    public static TracerProviderBuilder AddEvDbSinkSNSInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelSinkTrace.Name);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbSinkSNSInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkSNSMeters.Default.Name);
        return builder;
    }
}

