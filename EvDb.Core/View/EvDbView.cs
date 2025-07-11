using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

public abstract class EvDbView<TState> : EvDbView, IEvDbViewStore<TState>
{
    private readonly IEvDbTypedStorageSnapshotAdapter? _typedStorageAdapter;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshotResult snapshot,
        IEvDbStorageSnapshotAdapter storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, storageAdapter, timeProvider, logger, options, snapshot.StoredAt, snapshot.Offset)
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
        EvDbStoredSnapshotResult<TState> snapshot,
        IEvDbTypedStorageSnapshotAdapter typedStorageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, null, timeProvider, logger, options, snapshot.StoredAt, snapshot.Offset)
    {
        if (snapshot.Offset == 0)
            State = DefaultState;
        else
            State = snapshot.State;

        _typedStorageAdapter = typedStorageAdapter;
    }

    #endregion //  Ctor

    #region OnCustomSave

#if TYPED_STORAGE_ADAPTER
   protected override async Task<bool> OnCustomSave(CancellationToken cancellation)
    {
        if (_typedStorageAdapter == null || !_typedStorageAdapter.CanHandle<TState>(Address))
            return false;

        var snapshotData = new EvDbStoredSnapshotData<TState>(
                                        Address,
                                        MemoryOffset,
                                        StoreOffset,
                                        State);

        await _typedStorageAdapter.StoreSnapshotAsync(snapshotData, cancellation);
        return true;
    }
#endif

    #endregion //  OnCustomSave

    protected abstract TState DefaultState { get; }

    public TState State { get; protected set; }

    /// <summary>
    /// Prepare the snapshot serialized data.
    /// </summary>
    /// <returns></returns>
    public override EvDbStoredSnapshotData GetSnapshotData()
    {
        byte[] state = JsonSerializer.SerializeToUtf8Bytes(State, _options);
        var snapshot = new EvDbStoredSnapshotData(Address, MemoryOffset, StoreOffset, state);
        return snapshot;
    }

    #region NonStorageSnapshotAdapter

    private sealed class NonStorageSnapshotAdapter : IEvDbStorageSnapshotAdapter
    {
        public static readonly IEvDbStorageSnapshotAdapter Default = new NonStorageSnapshotAdapter();

        private NonStorageSnapshotAdapter()
        {
        }

        Task<EvDbStoredSnapshotResult> IEvDbStorageSnapshotAdapter.GetSnapshotAsync(EvDbViewAddress viewAddress, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        Task IEvDbStorageSnapshotAdapter.StoreSnapshotAsync(EvDbStoredSnapshotData snapshotData, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }

    #endregion //  NonStorageSnapshotAdapter
}

[DebuggerDisplay("Offset:[MemoryOffset:{MemoryOffset}], ShouldStore:[{ShouldStoreSnapshot}]")]
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
        DateTimeOffset? storedAt,
        long storedOffset = 0)
    {
        _storageAdapter = storageAdapter;
        TimeProvider = timeProvider;
        _logger = logger;
        _options = options;
        StoredAt = storedAt ?? DateTimeOffset.MinValue;
        StoreOffset = storedOffset;
        MemoryOffset = storedOffset;
        Address = address;
    }

    #endregion // Ctor

    /// <summary>
    /// Gets the unique address associated with the database view.
    /// </summary>
    public EvDbViewAddress Address { get; }

    /// <summary>
    /// Gets the <see cref="TimeProvider"/> instance used to retrieve the current time.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// The date and time when the item was stored or `DateTimeOffset.MinValue` when never been saved.
    /// </summary>
    public DateTimeOffset StoredAt { get; private set; }

    /// <summary>
    /// The last unsaved event offset (in the memory).
    /// </summary>
    public long MemoryOffset { get; private set; }

    /// <summary>
    /// The last persisted event offset.
    /// </summary>
    public long StoreOffset { get; set; }

    #region ShouldStoreSnapshot

    public virtual bool ShouldStoreSnapshot(long offsetGapFromLastSave, TimeSpan durationSinceLastSave) => true;

    #endregion // ShouldStoreSnapshot

    public abstract EvDbStoredSnapshotData GetSnapshotData();

    #region OnCustomSave

#if TYPED_STORAGE_ADAPTER
    /// <summary>
    /// Enable manual save of the snapshot.
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns>
    /// false: will use the default snapshot adapter to operate the save.
    /// true: assumed the snapshot was saved by the alternative implementation.
    /// </returns>
    protected abstract Task<bool> OnCustomSave(CancellationToken cancellation);
#endif

    #endregion //  OnCustomSave

    #region SaveAsync

    async Task IEvDbViewStore.SaveAsync(CancellationToken cancellation)
    {
        long numEventsSinceLatestSnapshot = StoreOffset == 0
                                                 ? MemoryOffset
                                                 : MemoryOffset - StoreOffset;
        TimeSpan durationSinceLatestSnapshot = TimeProvider.GetUtcNow() - StoredAt;
        if (!this.ShouldStoreSnapshot(numEventsSinceLatestSnapshot, durationSinceLatestSnapshot))
        {
            await Task.FromResult(true);
            return;
        }

        OtelTags tags = Address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.View.Store");
        using var duration = _sysMeters.MeasureStoreSnapshotsDuration(tags);

#if TYPED_STORAGE_ADAPTER
        await TypedStoreSnapshotAsync();
#else
        await StoreSnapshotAsync();
#endif // TYPED_STORAGE_ADAPTER

        _sysMeters.SnapshotStored.Add(1, tags);

        StoreOffset = MemoryOffset;
        StoredAt = TimeProvider.GetUtcNow();

        async Task StoreSnapshotAsync()
        {
            if (_storageAdapter == null)
                throw new MissingFieldException(nameof(_storageAdapter));
            EvDbStoredSnapshotData data = GetSnapshotData();
            await _storageAdapter.StoreSnapshotAsync(data, cancellation);
        }

        #region Task TypedStoreSnapshotAsync(){...}

#if TYPED_STORAGE_ADAPTER
        async Task TypedStoreSnapshotAsync()
        {
            bool saved = await OnCustomSave(cancellation);
            if (!saved)
            {
                await StoreSnapshotAsync();
            }
        }
#endif // TYPED_STORAGE_ADAPTER

        #endregion //  Task TypedStoreSnapshotAsync(){...}
    }

    #endregion //  StoreAsync

    #region ApplyEvent

    /// <summary>
    /// Append event into the view/aggregate.
    /// </summary>
    /// <param name="e"></param>
    public void ApplyEvent(EvDbEvent e)
    {
        long offset = e.StreamCursor.Offset;
        if (MemoryOffset >= offset)
            return;
        OnApplyEvent(e);
        MemoryOffset = offset;
    }

    #endregion // ApplyEvent

    #region OnApplyEvent

    /// <summary>
    /// Append event into the view/aggregate.
    /// </summary>
    /// <param name="e"></param>
    protected abstract void OnApplyEvent(EvDbEvent e);

    #endregion //  OnApplyEvent
}