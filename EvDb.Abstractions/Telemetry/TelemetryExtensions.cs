using EvDb.Core.Adapters;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Internals.OtelConstants;

namespace EvDb.Core;

public static class TelemetryExtensions
{
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

    #region StartActivity

    // TODO: [bnaya, 2025-06-17] builder pattern

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="tags"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Activity? StartActivity(this ActivitySource activitySource, OtelTags tags, [CallerMemberName] string name = "")
    {
        return activitySource.StartActivity(tags, ActivityKind.Internal, name);
    }

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="tags"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Activity? StartActivity(this ActivitySource activitySource, Func<OtelTags, OtelTags> tags, [CallerMemberName] string name = "")
    {
        var tagsResult = tags.Invoke(OtelTags.Empty);
        return activitySource.StartActivity(tagsResult, ActivityKind.Internal, name);
    }

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="tags"></param>
    /// <param name="kind"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Activity? StartActivity(this ActivitySource activitySource, Func<OtelTags, OtelTags> tags, ActivityKind kind = ActivityKind.Internal, [CallerMemberName] string name = "")
    {
        var tagsResult = tags.Invoke(OtelTags.Empty);
        return activitySource.StartActivity(tagsResult, kind, name);
    }

    /// <summary>
    /// Creates and starts a new <see cref="Activity"/> object if there is any listener to the Activity, returns null otherwise.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="tags"></param>
    /// <param name="kind"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Activity? StartActivity(this ActivitySource activitySource, OtelTags tags, ActivityKind kind = ActivityKind.Internal, [CallerMemberName] string name = "")
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

    #endregion // StartActivity

    #region CreateBuilder

    /// <summary>
    /// Creates Activity Builder for the given <see cref="ActivitySource"/> with the specified name.
    /// </summary>
    /// <param name="activitySource"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static ActivityBuilder CreateBuilder(this ActivitySource activitySource, [CallerMemberName] string name = "")
    {
        return new ActivityBuilder(activitySource, name);
    }

    #endregion //  CreateBuilder

    #region ActivityBuilder

    public readonly record struct ActivityBuilder(ActivitySource ActivitySource, [CallerMemberName] string Name = "")
    {
        public OtelTags Tags { get; init; } = OtelTags.Empty;
        public ActivityKind Kind { get; init; } = ActivityKind.Internal;

        public ActivityContext Parent { get; init; } = Activity.Current?.Context ?? default;

        public ActivityBuilder WithParent(ActivityContext parent)
        {
            return this with { Parent = parent };
        }

        public ActivityBuilder WithKind(ActivityKind kind)
        {
            return this with { Kind = kind };
        }

        public ActivityBuilder AddTag<T>(string key, T value)
        {
            return this with { Tags = Tags.Add(key, value) };
        }

        public Activity? Start()
        {
            var activity = ActivitySource.StartActivity(Name, Kind, Parent);
            if (activity != null)
            {
                foreach (var tag in Tags)
                {
                    activity.AddTag(tag.Key, tag.Value);
                }
            }
            return activity;
        }
    }

    #endregion //  ActivityBuilder

    #region ToTelemetryTags

    /// <summary>
    /// Convert `EvDbMessageRecord` to `OtelTags` for telemetry purposes.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="shard"></param>
    /// <returns></returns>
    public static OtelTags ToTelemetryTags(this EvDbMessageRecord message, EvDbShardName? shard = null)
    {
        var tags = OtelTags.Create(TAG_CHANNEL_NAME, message.Channel)
                        .Add(TAG_STREAM_TYPE, message.StreamType)
                        .Add(TAG_MESSAGE_TYPE_NAME, message.MessageType);

        if (shard.HasValue)
            tags = tags.Add(TAG_SHARD_NAME, shard.Value);

        return tags;
    }

    #endregion //  ToTelemetryTags
}

