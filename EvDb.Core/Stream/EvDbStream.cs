using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

// TODO:bnaya 2024-06-03 Add / View state should support fully immutable pattern for saving without locking
namespace EvDb.Core;


[DebuggerDisplay("{StreamAddress}, Stored Offset:{StoredOffset} ,Count:{CountOfPendingEvents}")]
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
        StoredOffset = lastStoredOffset;
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

        using var @lock = await _sync.AcquireAsync();
        var pending = _pendingEvents;
        EvDbStreamCursor cursor = GetNextCursor(pending);
        EvDbEvent e = new EvDbEvent(payload.EventType, TimeProvider.GetUtcNow(), capturedBy, cursor, json);
        _pendingEvents = pending.Add(e);

        #endregion // _pendingEvents

        foreach (IEvDbViewStore folding in _views)
            folding.FoldEvent(e);

        return e;

        EvDbStreamCursor GetNextCursor(IImmutableList<EvDbEvent> events)
        {
            if (events == ImmutableList<EvDbEvent>.Empty)
            {
                var empty = new EvDbStreamCursor(StreamAddress, StoredOffset + 1);
                return empty;
            }

            EvDbEvent e = events[^1];
            long offset = e.StreamCursor.Offset;
            var result = new EvDbStreamCursor(StreamAddress, offset + 1);
            return result;
        }
    }

    #endregion // AddEventAsync

    #region StoreAsync

    async Task<int> IEvDbStreamStore.StoreAsync(CancellationToken cancellation)
    {
        #region Telemetry

        OtelTags tags = OtelTags.Empty
                            .Add("evdb.domain", StreamAddress.Domain)
                            .Add("evdb.partition", StreamAddress.Partition);
        using var activity = _trace.StartActivity(tags, "EvDb.StoreAsync");

        using var duration = _sysMeters.MeasureStoreEventsDuration(tags);

        #endregion //  Telemetry

        using var @lock = await _sync.AcquireAsync();
        var events = _pendingEvents;
        if (events.Count == 0)
        {
            await Task.FromResult(true);
            return 0;
        }
        int affected = await _storageAdapter.StoreStreamAsync(events, this, cancellation);

        EvDbEvent ev = events[^1];
        StoredOffset = ev.StreamCursor.Offset;
        await Task.WhenAll(_views.Select(v => v.SaveAsync(cancellation)));
        _sysMeters.EventsStored.Add(events.Count, tags);

        using var clearPendingActivity = _trace.StartActivity(tags, "EvDb.ClearPendingEvents");
        var empty = ImmutableList<EvDbEvent>.Empty;
        _pendingEvents = events;
        foreach (var view in _views)
        {
            view.OnSaved();
        }

        return affected;
    }

    #endregion // StoreAsync

    #region StreamId

    public EvDbStreamAddress StreamAddress { get; init; }

    #endregion // StreamAddress

    #region CountOfPendingEvents

    /// <summary>
    /// number of events that were not stored yet.
    /// </summary>
    public int CountOfPendingEvents => _pendingEvents.Count;

    #endregion //  CountOfPendingEvents

    #region LastStoredOffset

    public long StoredOffset { get; protected set; }

    #endregion // StoredOffset

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


    #region Dispose Pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _sync.Dispose();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual void Dispose(bool disposed)
    {
    }


    /// <summary>
    /// Finalizes an instance of the <see cref="AsyncLock"/> class.
    /// </summary>
    ~EvDbStream()
    {
        Dispose(false);
    }

    #endregion // Dispose Pattern

}