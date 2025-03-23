using EvDb.Core.Adapters;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace EvDb.Core;

public static class StoreTelemetryExtensions
{
    public static TracerProviderBuilder AddEvDbStoreInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(StoreTelemetry.TraceName);
        return builder;
    }

    public static MeterProviderBuilder AddEvDbStoreInstrumentation(this MeterProviderBuilder builder,
                                                              EvDbMeters include = EvDbMeters.All)
    {
        builder.AddMeter(EvDbStoreMeters.MetricName);
        return builder;
    }
}

