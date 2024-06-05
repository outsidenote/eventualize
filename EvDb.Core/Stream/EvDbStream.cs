using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Text.Json;
using static EvDb.Core.Telemetry;
using System.Diagnostics;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

// TODO:bnaya 2024-06-03 Add / View state should support fully immutable pattern for saving without locking
namespace EvDb.Core;


[DebuggerDisplay("{StreamAddress}, Stored Offset:{StoreOffset} ,Count:{CountOfPendingEvents}")]
public abstract class EvDbStream :
    IEvDbStreamStore,
    IEvDbStreamStoreData
{
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;
    private const int ADDS_TRY_LIMIT = 10_000;
    private readonly AsyncLock _sync = new AsyncLock();

    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<EvDbEvent> _pendingEvents = ImmutableList<EvDbEvent>.Empty;

    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ??
                                                         throw new NotSupportedException("GetExecutingAssembly");

    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";

    private readonly IEvDbStorageAdapter _storageAdapter;

    #region Ctor

    public EvDbStream(
        IEvDbStreamConfig streamConfiguration,
        IImmutableList<IEvDbViewStore> views,
        IEvDbStorageAdapter storageAdapter,
        string streamId,
        long lastStoredOffset)
    {
        _views = views;
        _storageAdapter = storageAdapter;
        StreamAddress = new EvDbStreamAddress(streamConfiguration.PartitionAddress, streamId);
        StoreOffset = lastStoredOffset;
        Options = streamConfiguration.Options;
        TimeProvider = streamConfiguration.TimeProvider ?? TimeProvider.System;
    }

    #endregion // Ctor

    #region Views

    protected readonly IImmutableList<IEvDbViewStore> _views;
    IEnumerable<IEvDbView> IEvDbStreamStoreData.Views => _views.Cast<IEvDbView>();

    #endregion // Views

    #region AddEventAsync

    protected async ValueTask<IEvDbEventMeta> AddEventAsync<T>(T payload, string? capturedBy = null)
        where T : IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(payload, Options);

        #region _pendingEvents = _pendingEvents.Add(e)

        int i;
        EvDbEvent e = EvDbEvent.Empty;

        for (i = 0; i < ADDS_TRY_LIMIT; i++)
        {
            var compare = _pendingEvents;
            EvDbStreamCursor cursor = GetNextCursor(compare);
            e = new EvDbEvent(payload.EventType, TimeProvider.GetUtcNow(), capturedBy, cursor, json);
            var newEvents = compare.Add(e);
            if (Interlocked.CompareExchange(ref _pendingEvents, newEvents, compare) == compare)
                break;
            if(i > 10 && i % 5 == 0)
                await Task.Yield();
        }
        if (i >= ADDS_TRY_LIMIT)
            throw new OperationCanceledException("Fail to add event to the stream");

        #endregion // _pendingEvents

        foreach (IEvDbViewStore folding in _views)
            folding.FoldEvent(e);

        return e;

        EvDbStreamCursor GetNextCursor(IImmutableList<EvDbEvent> events)
        {
            if (events == ImmutableList<EvDbEvent>.Empty)
            {
                var empty = new EvDbStreamCursor(StreamAddress, StoreOffset + 1);
                return empty;
            }

            EvDbEvent e = events[^1];
            long offset = e.StreamCursor.Offset;
            var result = new EvDbStreamCursor(StreamAddress, offset + 1);
            return result;
        }
    }

    #endregion // AddEventAsync

    #region SaveAsync

    async Task IEvDbStreamStore.SaveAsync(CancellationToken cancellation)
    {
        OtelTags tags = OtelTags.Empty
                            .Add("evdb.domain", StreamAddress.Domain)
                            .Add("evdb.partition", StreamAddress.Partition);
        using var activity = _trace.StartActivity(tags, "EvDb.SaveAsync");
        if (!this.HasPendingEvents)
        {
            await Task.FromResult(true);
            return;
        }

        using var duration = _sysMeters.MeasureStoreEventsDuration(tags);

        // TODO: lock Adds reader writer lock
        var events = _pendingEvents;
        await _storageAdapter.SaveStreamAsync(events, this, cancellation);
        EvDbEvent ev = events[^1];
        StoreOffset = ev.StreamCursor.Offset;
        await Task.WhenAll(_views.Select(v => v.SaveAsync(cancellation)));
        _sysMeters.EventsStored.Add(events.Count, tags);

        using var clearPendingActivity = _trace.StartActivity(tags, "EvDb.ClearPendingEvents");
        var empty = ImmutableList<EvDbEvent>.Empty;
        if (Interlocked.CompareExchange(ref _pendingEvents, empty, events) != events)
        {
            int i;
            for (i = 0; i < 1000; i++)
            {
                var compare = _pendingEvents;
                var newEvents = _pendingEvents.RemoveRange(events);
                if (Interlocked.CompareExchange(ref _pendingEvents, newEvents, compare) == compare)
                    break;
            }
            if (i >= 1000)
                throw new OperationCanceledException("Fail to update the stream events");
        }
        foreach (var view in _views)
        {
            view.OnSaved();
        }
    }

    #endregion // SaveAsync

    #region StreamId

    public EvDbStreamAddress StreamAddress { get; init; }

    #endregion // StreamAddress

    public int CountOfPendingEvents => _pendingEvents.Count;

    #region LastStoredOffset

    public long StoreOffset { get; protected set; }

    #endregion // StoreOffset

    #region IsEmpty

    public bool HasPendingEvents => _pendingEvents.Count > 0;

    #endregion // HasPendingEvents

    #region Options

    /// <summary>
    /// Serialization options
    /// </summary>
    public JsonSerializerOptions? Options { get; }

    #endregion // Options

    #region TimeProvider

    /// <summary>
    /// Time abstraction
    /// </summary>
    public TimeProvider TimeProvider { get; }

    #endregion // TimeProvider

    #region IEvDbStreamStoreData.Events

    /// <summary>
    /// Unspecialized events
    /// </summary>
    public IEnumerable<EvDbEvent> Events => _pendingEvents;

    #endregion // IEvDbStreamStoreData.Events
}