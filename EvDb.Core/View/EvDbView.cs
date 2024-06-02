using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

public abstract class EvDbView<T> : EvDbView, IEvDbViewStore<T>
{
    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshot snapshot,
        IEvDbStorageAdapter storageAdapter,
        TimeProvider timeProvider,
        ILogger logger,
        JsonSerializerOptions? options) :
        base(address, storageAdapter, timeProvider, logger, options, snapshot.Offset)
    {
        if (snapshot == EvDbStoredSnapshot.Empty || string.IsNullOrEmpty(snapshot.State))
            State = DefaultState;
        else
        {
            State = JsonSerializer.Deserialize<T>(snapshot.State, options) ?? DefaultState;
        }
    }

    protected abstract T DefaultState { get; }

    public virtual T State { get; protected set; }

    public override EvDbStoredSnapshotAddress GetSnapshot()
    {
        string state = JsonSerializer.Serialize<T>(State, _options);
        var snapshot = new EvDbStoredSnapshotAddress(Address, FoldOffset, state);
        return snapshot;
    }
}

[DebuggerDisplay("Offset:[Stored: {StoreOffset}, Folded:{FoldOffset}], ShouldStore:[{ShouldStoreSnapshot}]")]
public abstract class EvDbView : IEvDbViewStore
{
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    private readonly IEvDbStorageAdapter _storageAdapter;
    protected readonly JsonSerializerOptions? _options;
    protected readonly ILogger _logger;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        IEvDbStorageAdapter storageAdapter,
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

    public abstract EvDbStoredSnapshotAddress GetSnapshot();

    public async Task SaveAsync(CancellationToken cancellation = default)
    {
        OtelTags tags = OtelTags.Empty
                                .Add("evdb.domain", Address.Domain)
                                .Add("evdb.partition", Address.Partition)
                                .Add("evdb.view.name", Address.ViewName);
        using var activity = _trace.StartActivity(tags, "EvDb.View.SaveAsync");

        if (!this.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }
        using var duration = _sysMeters.MeasureStoreSnapshotsDuration(tags);
        await this._storageAdapter.SaveViewAsync(this, cancellation);
        _sysMeters.SnapshotStored.Add(1, tags);
        return;
    }

    public EvDbViewAddress Address { get; }

    public TimeProvider TimeProvider { get; }

    public long FoldOffset { get; private set; }

    public long StoreOffset { get; set; }

    public virtual int MinEventsBetweenSnapshots => 0;

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