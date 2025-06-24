using EvDb.Sinks.EvDbSinkSQS;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryCoreExtensions
{
    public static TracerProviderBuilder AddEvDbSinkSQSInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelTrace.Name);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbSinkSQSInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkSQSMeters.Default.Name);
        return builder;
    }
}

