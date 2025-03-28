﻿using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EvDb.Core;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

internal class EvDbSysMeters : IEvDbSysMeters
{
    public const string MetricCounterName = "EvDb.Counters";
    public const string MetricDurationName = "EvDb.Durations";

    public EvDbSysMeters() : this(
        new Meter(MetricCounterName),
        new Meter(MetricDurationName)
        )
    {
    }

    //public EvDbSysMeters(
    //              [FromKeyedServices(MetricCounterName)]IMeterFactory meterFactory,
    //              [FromKeyedServices(MetricDurationName)]IMeterFactory meterDurationFactory
    //              ): this(meterFactory.Create(MetricCounterName))
    //{
    //}

    private EvDbSysMeters(Meter counterMeter, Meter durationMeter)
    {
        OCC = counterMeter.CreateCounter<int>("evdb_occ",
            "{collision}",
            "Optimistic Concurrency Collisions");
        EventsStored = counterMeter.CreateCounter<int>("evdb_events_stored",
            "{event}",
            "Events stored into the storage database");
        MessagesStored = counterMeter.CreateCounter<int>("evdb_outbox_messagess_stored",
            "{messages}",
            "Outbox messages stored into the storage database");
        SnapshotStored = counterMeter.CreateCounter<int>(
            "evdb_snapshot_stored",
            "{snapshot}", "Snapshot stored into the storage database");

        _factoryGetDuration = durationMeter.CreateHistogram<double>(
            "evdb_factory_get-duration",
            "ms",
            "Durations of factory get");

        _eventsStoredDuration = durationMeter.CreateHistogram<double>(
            "evdb_events_stored_duration",
            "ms",
            "Durations of events stored into the storage database");
        _snapshotStoredDuration = durationMeter.CreateHistogram<double>(
            "evdb_snapshot_stored_duration",
            "ms",
            "Durations of snapshot stored into the storage database");
    }

    /// <summary>
    /// Optimistic Concurrency Collisions
    /// </summary>
    public Counter<int> OCC { get; }

    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    public Counter<int> EventsStored { get; }

    /// <summary>
    /// Outbox messages stored into the storage database
    /// </summary>
    public Counter<int> MessagesStored { get; }

    /// <summary>
    /// Snapshot stored into the storage database
    /// </summary>
    public Counter<int> SnapshotStored { get; }

    /// <summary>
    /// Durations of factory get
    /// </summary>
    private readonly Histogram<double> _factoryGetDuration;

    /// <summary>
    /// Durations of events stored into the storage database
    /// </summary>
    private readonly Histogram<double> _eventsStoredDuration;
    /// <summary>
    /// Durations of snapshot stored into the storage database
    /// </summary>
    private readonly Histogram<double> _snapshotStoredDuration;

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    public IDisposable MeasureFactoryGetDuration(Func<OtelTags, OtelTags>? action = null)
    {
        if (!_factoryGetDuration.Enabled)
            return Disposable.Empty;

        OtelTags tags = OtelTags.Empty;
        tags = action?.Invoke(tags) ?? tags;

        return MeasureFactoryGetDuration(tags);
    }

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public IDisposable MeasureFactoryGetDuration(OtelTags tags)
    {
        if (!_factoryGetDuration.Enabled)
            return Disposable.Empty;

        var sw = Stopwatch.StartNew();
        return Disposable.Create(() =>
        {
            sw.Stop();
            _factoryGetDuration.Record(sw.ElapsedMilliseconds, tags);
        });
    }

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    public IDisposable MeasureStoreEventsDuration(Func<OtelTags, OtelTags>? action = null)
    {
        if (!_eventsStoredDuration.Enabled)
            return Disposable.Empty;

        OtelTags tags = OtelTags.Empty;
        tags = action?.Invoke(tags) ?? tags;

        return MeasureStoreEventsDuration(tags);
    }

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public IDisposable MeasureStoreEventsDuration(OtelTags tags)
    {
        if (!_eventsStoredDuration.Enabled)
            return Disposable.Empty;

        var sw = Stopwatch.StartNew();
        return Disposable.Create(() =>
        {
            sw.Stop();
            _eventsStoredDuration.Record(sw.ElapsedMilliseconds, tags);
        });
    }

    /// <summary>
    /// Measure the duration of Store Snapshot
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    public IDisposable MeasureStoreSnapshotsDuration(Func<OtelTags, OtelTags>? action = null)
    {
        if (!_snapshotStoredDuration.Enabled)
            return Disposable.Empty;

        OtelTags tags = OtelTags.Empty;
        tags = action?.Invoke(tags) ?? tags;

        return MeasureStoreSnapshotsDuration(tags);
    }

    /// <summary>
    /// Measure the duration of Store Snapshot
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public IDisposable MeasureStoreSnapshotsDuration(OtelTags tags)
    {
        if (!_snapshotStoredDuration.Enabled)
            return Disposable.Empty;

        var sw = Stopwatch.StartNew();
        return Disposable.Create(() =>
        {
            sw.Stop();
            _snapshotStoredDuration.Record(sw.ElapsedMilliseconds, tags);
        });
    }
}
