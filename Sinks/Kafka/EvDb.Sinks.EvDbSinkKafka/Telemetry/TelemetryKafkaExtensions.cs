using EvDb.Sinks.EvDbSinkKafka;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryKafkaExtensions
{
    public static TracerProviderBuilder AddEvDbSinkSQSInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelSinkTrace.Name);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbSinkSQSInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkKafkaMeters.Default.Name);
        return builder;
    }
}

