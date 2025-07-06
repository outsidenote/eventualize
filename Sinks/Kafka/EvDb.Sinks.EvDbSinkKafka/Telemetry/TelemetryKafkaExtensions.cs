using EvDb.Sinks.EvDbSinkKafka;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace Microsoft.Extensions.DependencyInjection;

public static class TelemetryKafkaExtensions
{
    public static TracerProviderBuilder AddEvDbSinkKafkaInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(OtelSinkTrace.Name)
               .AddSource("Confluent.Kafka.Extensions.Diagnostics"); 
        return builder;
    }

    public static MeterProviderBuilder AddEvDbSinkKafkaInstrumentation(this MeterProviderBuilder builder)
    {
        builder.AddMeter(EvDbSinkKafkaMeters.Default.Name)
               .AddMeter("Confluent.Kafka.Extensions.Diagnostics");
        return builder;
    }
}

