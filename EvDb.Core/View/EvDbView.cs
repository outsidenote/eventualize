using System.Diagnostics;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

public abstract class EvDbView<T> : EvDbView, IEvDbViewStore<T>
{
    protected EvDbView(
        EvDbViewAddress address,
        EvDbStoredSnapshot snapshot,
        IEvDbStorageAdapter storageAdapter,
        JsonSerializerOptions? options) :
        base(address, storageAdapter, options, snapshot.Offset)
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
    private readonly IEvDbStorageAdapter _storageAdapter;
    protected readonly JsonSerializerOptions? _options;

    #region Ctor

    protected EvDbView(
        EvDbViewAddress address,
        IEvDbStorageAdapter storageAdapter,
        JsonSerializerOptions? options,
        long storedOffset = -1)
    {
        _storageAdapter = storageAdapter;
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
        if (!this.ShouldStoreSnapshot)
        {
            await Task.FromResult(true);
            return;
        }

        await this._storageAdapter.SaveViewAsync(this, cancellation);
        return;
    }

    public EvDbViewAddress Address { get; }

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