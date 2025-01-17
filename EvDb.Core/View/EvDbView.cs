﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public abstract class EvDbView<T> : EvDbView, IEvDbViewStore<T>
{
    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshot snapshot,
        IEvDbStorageSnapshotAdapter storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, storageAdapter, timeProvider, logger, options, snapshot.Offset)
    {
        if (snapshot == EvDbStoredSnapshot.Empty || snapshot.State.Length == 0)
            State = DefaultState;
        else
        {
            State = JsonSerializer.Deserialize<T>(snapshot.State, options) ?? DefaultState;
        }
    }

    protected abstract T DefaultState { get; }

    public T State { get; protected set; }

    /// <summary>
    /// Prepare the snapshot serialized data.
    /// </summary>
    /// <returns></returns>
    public override EvDbStoredSnapshotData GetSnapshotData()
    {
        byte[] state = JsonSerializer.SerializeToUtf8Bytes(State, _options);
        var snapshot = new EvDbStoredSnapshotData(Address, FoldOffset, state);
        return snapshot;
    }
}

[DebuggerDisplay("Offset:[Folded:{FoldOffset}], ShouldStore:[{ShouldStoreSnapshot}]")]
public abstract class EvDbView : IEvDbViewStore
{
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    private readonly IEvDbStorageSnapshotAdapter _storageAdapter;
    protected readonly JsonSerializerOptions? _options;
    protected readonly ILogger _logger;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        IEvDbStorageSnapshotAdapter storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options,
        long storedOffset = -1)
    {
        _storageAdapter = storageAdapter;
        TimeProvider = timeProvider;
        _logger = logger;
        _options = options;
        StoreOffset = storedOffset;
        FoldOffset = storedOffset;
        Address = address;
    }

    #endregion // Ctor

    public EvDbViewAddress Address { get; }

    public TimeProvider TimeProvider { get; }

    public long FoldOffset { get; private set; }

    public long StoreOffset { get; set; }

    public virtual int MinEventsBetweenSnapshots => 0;

    #region OnSaved

    public void OnSaved()
    {
        if (ShouldStoreSnapshot)
            StoreOffset = FoldOffset;
    }

    #endregion // OnSaved

    #region ShouldStoreSnapshot

    public bool ShouldStoreSnapshot
    {
        get
        {
            long numEventsSinceLatestSnapshot = StoreOffset == -1
                ? FoldOffset
                : FoldOffset - StoreOffset;
            bool result = numEventsSinceLatestSnapshot >= MinEventsBetweenSnapshots;
            return result;
        }
    }

    #endregion // ShouldStoreSnapshot

    public abstract EvDbStoredSnapshotData GetSnapshotData();

    #region SaveAsync

    public async Task SaveAsync(CancellationToken cancellation = default)
    {
        if (!this.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }

        OtelTags tags = Address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.View.StoreAsync");
        using var duration = _sysMeters.MeasureStoreSnapshotsDuration(tags);
        await this._storageAdapter.StoreViewAsync(this, cancellation);
        _sysMeters.SnapshotStored.Add(1, tags);
    }

    #endregion //  StoreAsync

    #region FoldEvent

    public void FoldEvent(EvDbEvent e)
    {
        long offset = e.StreamCursor.Offset;
        if (FoldOffset >= offset)
            return;
        OnFoldEvent(e);
        FoldOffset = offset;
    }

    #endregion // FoldEvent

    protected abstract void OnFoldEvent(EvDbEvent e);
}