﻿// Ignore Spelling: OutboxHandler Outbox

using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<TStreamFactory> which is a wrapper of TStreamFactory that holds TStreamFactory and snapshotOffset 

// TODO:bnaya 2024-06-03 AddToOutbox / View state should support fully immutable pattern for saving without locking
namespace EvDb.Core;


[DebuggerDisplay("{StreamAddress}, Stored Offset:{StoredOffset} ,Count:{CountOfPendingEvents}")]
public abstract class EvDbStream :
    IEvDbStreamStore,
    IEvDbStreamStoreData
{
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;
    private readonly AsyncLock _sync = new AsyncLock();


    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<EvDbEvent> _pendingEvents = ImmutableList<EvDbEvent>.Empty;
    protected internal IImmutableList<EvDbOutboxEntity> _pendingOutput = ImmutableList<EvDbOutboxEntity>.Empty;

    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ??
                                                         throw new NotSupportedException("GetExecutingAssembly");

    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";

    private readonly IEvDbStorageStreamAdapter _storageAdapter;

    #region Ctor

    public EvDbStream(
        IEvDbStreamConfig streamConfiguration,
        IImmutableList<IEvDbViewStore> views,
        IEvDbStorageStreamAdapter storageAdapter,
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
        where T : IEvDbPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(payload, Options);

        #region _pendingEvents = _pendingEvents.Add(e)

        using var @lock = await _sync.AcquireAsync();
        var pending = _pendingEvents;
        EvDbStreamCursor cursor = GetNextCursor(pending);
        EvDbEvent e = new EvDbEvent(payload.PayloadType, TimeProvider.GetUtcNow(), capturedBy, cursor, json);
        _pendingEvents = pending.Add(e);

        #endregion // _pendingEvents

        foreach (IEvDbViewStore folding in _views)
            folding.FoldEvent(e);

        OutboxHandler?.OnOutboxHandler(e, _views);

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

    #region OutboxHandler

    /// <summary>
    /// Gets the outbox pattern handler.
    /// </summary>
    protected virtual IEvDbOutboxHandler? OutboxHandler { get; }

    #endregion //  OutboxHandler

    #region AddToOutbox

    /// <summary>
    /// Put a row into the publication (out-box pattern).
    /// </summary>
    /// <param name="e">The e.</param>
    public void AddToOutbox(EvDbOutboxEntity e)
    {
        _pendingOutput = _pendingOutput.Add(e);
    }

    #endregion //  AddToOutbox

    #region StoreAsync

    async Task<int> IEvDbStreamStore.StoreAsync(CancellationToken cancellation)
    {
        #region Telemetry

        OtelTags tags = StreamAddress.ToOtelTagsToOtelTags();

        using var duration = _sysMeters.MeasureStoreEventsDuration(tags);
        using var activity = _trace.StartActivity(tags, "EvDb.StoreAsync");

        #endregion //  Telemetry

        using var @lock = await _sync.AcquireAsync();
        var events = _pendingEvents;
        var outbox = _pendingOutput;
        if (events.Count == 0)
        {
            await Task.FromResult(true);
            return 0;
        }
        try
        {
            // TODO: bnaya 2024-09-16 add the outbox into the StoreStreamAsync
            int affected = await _storageAdapter.StoreStreamAsync(events, outbox, this, cancellation);
            _sysMeters.EventsStored.Add(affected, tags);

            EvDbEvent ev = events[^1];
            StoredOffset = ev.StreamCursor.Offset;
            var viewSaveTasks = _views.Select(v => v.SaveAsync(cancellation));
            await Task.WhenAll(viewSaveTasks);

            using var clearPendingActivity = _trace.StartActivity(tags, "EvDb.ClearPendingEvents");
            _pendingEvents = ImmutableList<EvDbEvent>.Empty;
            _pendingOutput = ImmutableList<EvDbOutboxEntity>.Empty;
            foreach (IEvDbViewStore view in _views)
                view.OnSaved();

            return affected;
        }
        catch (OCCException)
        {
            _sysMeters.OCC.Add(1);
            throw;
        }
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