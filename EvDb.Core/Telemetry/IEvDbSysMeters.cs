using System.Diagnostics.Metrics;

namespace EvDb.Core;

internal interface IEvDbSysMeters
{
    Counter<int> EventsStored { get; }
    Counter<int> SnapshotStored { get; }
}
