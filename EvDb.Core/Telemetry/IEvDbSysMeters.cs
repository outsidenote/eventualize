using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EvDb.Core;

internal interface IEvDbSysMeters
{
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    Counter<int> EventsStored { get; }

    /// <summary>
    /// Optimistic Concurrency Collisions
    /// </summary>
    Counter<int> OCC { get; }

    /// <summary>
    /// Snapshot stored into the storage database
    /// </summary>
    Counter<int> SnapshotStored { get; }

    /// <summary>
    /// Measure the duration of Factory Get
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    IDisposable MeasureFactoryGetDuration(Func<OtelTags, OtelTags>? action = null);

    /// <summary>
    /// Measure the duration of Factory Get
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    IDisposable MeasureFactoryGetDuration(OtelTags tags);

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    IDisposable MeasureStoreEventsDuration(Func<OtelTags, OtelTags>? action = null);

    /// <summary>
    /// Measure the duration of Store Events
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    IDisposable MeasureStoreEventsDuration(OtelTags tags);

    /// <summary>
    /// Measure the duration of Store Snapshot
    /// </summary>
    /// <param name="action">Attach tags</param>
    /// <returns></returns>
    IDisposable MeasureStoreSnapshotsDuration(Func<OtelTags, OtelTags>? action = null);

    /// <summary>
    /// Measure the duration of Store Snapshot
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    IDisposable MeasureStoreSnapshotsDuration(OtelTags tags);

}
