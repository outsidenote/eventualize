using System.Diagnostics.Metrics;

namespace EvDb.Core;

internal class EvDbSysMeters : IEvDbSysMeters
{
    public const string MetricName = "EvDb";

    public EvDbSysMeters(): this(new Meter(MetricName))
    { 
    }

    //public EvDbSysMeters(IMeterFactory meterFactory): this(meterFactory.Create(MetricName))
    //{
    //}

    private EvDbSysMeters(Meter meter)
    {
        EventsStored = meter.CreateCounter<int>("events-stored", "{event}", "Events stored into the storage database");
        SnapshotStored = meter.CreateCounter<int>("snapshot-stored", "{snapshot}", "Snapshot stored into the storage database");
    }

    public Counter<int> EventsStored { get; }
    public Counter<int> SnapshotStored { get; }
}
