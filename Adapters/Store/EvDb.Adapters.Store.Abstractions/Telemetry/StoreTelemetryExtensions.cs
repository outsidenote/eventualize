using EvDb.Core.Adapters;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using static EvDb.Core.Adapters.StoreTelemetry;
using static EvDb.Core.Internals.OtelConstants;

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

    #region StartFetchFromOutboxActivity

    public static Activity? StartFetchFromOutboxActivity(this EvDbMessageRecord message, EvDbShardName shard, string databaseType)
    {
        var telemetryContext = message.TelemetryContext.ToTelemetryContext();
        var activity = StoreTrace.StartActivity(ActivityKind.Consumer, 
                                    name: "EvDb.FetchedFromOutbox",
                                    links: new[] {  new ActivityLink(telemetryContext) },
                                    tags: message.ToTelemetryTags(shard)
                                                 .Add(TAG_STORAGE_TYPE_NAME, databaseType));
        return activity;
    }

    #endregion //  StartFetchFromOutboxActivity
}

