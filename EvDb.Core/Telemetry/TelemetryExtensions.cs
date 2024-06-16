using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace EvDb.Core;

[Flags]
public enum EvDbMeters
{
    None,
    SystemCounters = 1,
    SystemDuration = SystemCounters * 2,
    All = SystemCounters | SystemDuration,
}

public static class TelemetryExtensions
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

    #region Counter

    /// <summary>
    /// Record the increment value of the measurement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="counter"></param>
    /// <param name="count"></param>
    /// <param name="action"></param>
    public static void Add<T>(this Counter<T> counter, T count, Func<OtelTags, OtelTags> action)
        where T : struct
    {
        var tags = action.Invoke(OtelTags.Empty);
        counter.Add(count, tags);
    }

    /// <summary>
    /// Record the increment value of the measurement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="counter"></param>
    /// <param name="count"></param>
    /// <param name="tags"></param>
    public static void Add<T>(this Counter<T> counter, T count, OtelTags tags)
        where T : struct
    {
        TagList tagList = tags;
        counter.Add(count, in tagList);
    }

    #endregion // Counter

    #region Record

    /// <summary>
    /// Record a measurement value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="histogram"></param>
    /// <param name="value"></param>
    /// <param name="action"></param>
    public static void Record<T>(this Histogram<T> histogram, T value, Func<OtelTags, OtelTags> action)
        where T : struct
    {
        var tags = action.Invoke(OtelTags.Empty);
        histogram.Record<T>(value, tags);
    }

    /// <summary>
    /// Record a measurement value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="histogram"></param>
    /// <param name="value"></param>
    /// <param name="tags"></param>
    public static void Record<T>(this Histogram<T> histogram, T value, OtelTags tags)
        where T : struct
    {
        TagList tagList = tags;
        histogram.Record(value, in tagList);
    }

    #endregion // Record

    #region Record

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="tags"></param>
    /// <param name="name"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static Activity? StartActivity(this ActivitySource activitySource, OtelTags tags, [CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal)
    {
        var activity = activitySource.StartActivity(name, kind);
        if (activity != null)
        {
            foreach (var tag in tags)
            {
                activity.AddTag(tag.Key, tag.Value);
            }
        }
        return activity;
    }

    #endregion // Record

}

