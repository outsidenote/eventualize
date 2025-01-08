﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public abstract class EvDbView<TState> : EvDbView, IEvDbViewStore<TState>
{
    private readonly IEvDbStorageSnapshotAdapter<TState>? _typedStorageAdapter;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshot snapshot,
        IEvDbStorageSnapshotAdapter storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, storageAdapter, timeProvider, logger, options, snapshot.Offset)
    {
        if (snapshot.Offset == 0)
            State = DefaultState;
        else
        {
            State = JsonSerializer.Deserialize<TState>(snapshot.State, options) ?? DefaultState;
        }
    }

    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshot<TState> snapshot,
        IEvDbStorageSnapshotAdapter<TState> typedStorageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, null, timeProvider, logger, options, snapshot.Offset)
    {
        if (snapshot.Offset == 0)
            State = DefaultState;
        else
            State = snapshot.State;

        _typedStorageAdapter = typedStorageAdapter;
    }

    #endregion //  Ctor

    protected override async Task<bool> OnSave(CancellationToken cancellation)
    {
        if (_typedStorageAdapter == null)
            return false;

        var snapshotData = new EvDbStoredSnapshotData<TState>(Address, FoldOffset, State);

        await _typedStorageAdapter.StoreViewAsync(snapshotData, cancellation);
        return true;
    }

    protected abstract TState DefaultState { get; }

    public TState State { get; protected set; }

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

    #region NonStorageSnapshotAdapter

    private sealed class NonStorageSnapshotAdapter : IEvDbStorageSnapshotAdapter
    {
        public static readonly IEvDbStorageSnapshotAdapter Default = new NonStorageSnapshotAdapter();

        private NonStorageSnapshotAdapter()
        {
        }

        Task<EvDbStoredSnapshot> IEvDbStorageSnapshotAdapter.GetSnapshotAsync(EvDbViewAddress viewAddress, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        Task IEvDbStorageSnapshotAdapter.StoreViewAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }

    #endregion //  NonStorageSnapshotAdapter
}

[DebuggerDisplay("Offset:[Folded:{FoldOffset}], ShouldStore:[{ShouldStoreSnapshot}]")]
public abstract class EvDbView : IEvDbViewStore
{
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    private readonly IEvDbStorageSnapshotAdapter? _storageAdapter;
    protected readonly JsonSerializerOptions? _options;
    protected readonly ILogger _logger;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        IEvDbStorageSnapshotAdapter? storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options,
        long storedOffset = 0)
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

    #region ShouldStoreSnapshot

    public bool ShouldStoreSnapshot
    {
        get
        {
            long numEventsSinceLatestSnapshot = StoreOffset == 0
                ? FoldOffset
                : FoldOffset - StoreOffset;
            bool result = numEventsSinceLatestSnapshot > MinEventsBetweenSnapshots;
            return result;
        }
    }

    #endregion // ShouldStoreSnapshot

    public abstract EvDbStoredSnapshotData GetSnapshotData();

    protected abstract Task<bool> OnSave(CancellationToken cancellation);

    #region SaveAsync

    async Task IEvDbViewStore.SaveAsync(CancellationToken cancellation)
    {
        if (!this.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }
        OtelTags tags = Address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.View.StoreAsync");
        using var duration = _sysMeters.MeasureStoreSnapshotsDuration(tags);

        bool saved = await OnSave(cancellation);
        if (!saved)
        {
            if(_storageAdapter == null)
                throw new NullReferenceException(nameof(_storageAdapter));
            EvDbStoredSnapshotData data = GetSnapshotData();
            await _storageAdapter.StoreViewAsync(data, cancellation);
        }
        _sysMeters.SnapshotStored.Add(1, tags);

        StoreOffset = FoldOffset;
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