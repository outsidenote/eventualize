using EvDb.Sinks.EvDbSinkMassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryMassTransitExtensions
{
    public static TracerProviderBuilder AddEvDbSinkMassTransitInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelSinkTrace.Name);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbSinkMassTransitInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkMassTransitMeters.Default.Name);
        return builder;
    }
}

