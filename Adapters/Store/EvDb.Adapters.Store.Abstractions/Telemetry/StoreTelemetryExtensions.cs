using EvDb.Core.Adapters;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Buffers;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public static class StoreTelemetryExtensions
{
    #region AddEvDbStoreInstrumentation

    public static TracerProviderBuilder AddEvDbStoreInstrumentation(this TracerProviderBuilder builder)
    {
        builder.AddSource(StoreTelemetry.TraceName);
        return builder;
    }

    #endregion //  AddEvDbStoreInstrumentation

    #region AddEvDbStoreInstrumentation

    public static MeterProviderBuilder AddEvDbStoreInstrumentation(this MeterProviderBuilder builder,
                                                              EvDbMeters include = EvDbMeters.All)
    {
        builder.AddMeter(EvDbStoreMeters.MetricName);
        return builder;
    }

    #endregion //  AddEvDbStoreInstrumentation
}

