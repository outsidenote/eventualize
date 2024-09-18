using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EvDb.Core.Adapters;

// https://www.mytechramblings.com/posts/getting-started-with-opentelemetry-metrics-and-dotnet-part-1/

internal class EvDbStoreMeters : IEvDbStoreMeters
{
    public const string MetricName = "EvDb.Store.Relational";

    public EvDbStoreMeters(IMeterFactory meterFactory) : this(
        meterFactory.Create(MetricName))
    {
    }

    //public EvDbSysMeters(
    //              [FromKeyedServices(MetricCounterName)]IMeterFactory meterFactory,
    //              [FromKeyedServices(MetricDurationName)]IMeterFactory meterDurationFactory
    //              ): this(meterFactory.Create(MetricCounterName))
    //{
    //}

    private EvDbStoreMeters(Meter counterMeter)
    {
        //OCC = counterMeter.CreateCounter<int>("evdb_occ",
        //    "{collision}",
        //    "Optimistic Concurrency Collisions");
        //EventsStored = counterMeter.CreateCounter<int>("evdb_events_stored",
        //    "{event}",
        //    "Events stored into the storage database");
        //SnapshotStored = counterMeter.CreateCounter<int>(
        //    "evdb_snapshot_stored",
        //    "{snapshot}", "Snapshot stored into the storage database");

        //_factoryGetDuration = durationMeter.CreateHistogram<double>(
        //    "evdb_factory_get-duration",
        //    "ms",
        //    "Durations of factory get");

        //_eventsStoredDuration = durationMeter.CreateHistogram<double>(
        //    "evdb_events_stored_duration",
        //    "ms",
        //    "Durations of events stored into the storage database");
        //_snapshotStoredDuration = durationMeter.CreateHistogram<double>(
        //    "evdb_snapshot_stored_duration",
        //    "ms",
        //    "Durations of snapshot stored into the storage database");
    }

    /// <summary>
    /// Number of events stored
    /// </summary>
    private readonly Counter<int> EventsStored;

    ///// <summary>
    ///// Number of outbox stored
    ///// </summary>
    //private readonly Counter<int> OutboxStored { get; }
    ///// <summary>
    ///// Snapshot stored into the storage database
    ///// </summary>
    //private readonly Counter<int> SnapshotStored { get; }

    Counter<int> IEvDbStoreMeters.EventsStored => throw new NotImplementedException();

    Counter<int> IEvDbStoreMeters.OCC => throw new NotImplementedException();

    Counter<int> IEvDbStoreMeters.SnapshotStored => throw new NotImplementedException();

    IDisposable IEvDbStoreMeters.MeasureFactoryGetDuration(Func<OtelTags, OtelTags>? action)
    {
        throw new NotImplementedException();
    }

    IDisposable IEvDbStoreMeters.MeasureFactoryGetDuration(OtelTags tags)
    {
        throw new NotImplementedException();
    }

    IDisposable IEvDbStoreMeters.MeasureStoreEventsDuration(Func<OtelTags, OtelTags>? action)
    {
        throw new NotImplementedException();
    }

    IDisposable IEvDbStoreMeters.MeasureStoreEventsDuration(OtelTags tags)
    {
        throw new NotImplementedException();
    }

    IDisposable IEvDbStoreMeters.MeasureStoreSnapshotsDuration(Func<OtelTags, OtelTags>? action)
    {
        throw new NotImplementedException();
    }

    IDisposable IEvDbStoreMeters.MeasureStoreSnapshotsDuration(OtelTags tags)
    {
        throw new NotImplementedException();
    }
}
