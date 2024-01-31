using System.Diagnostics;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

public abstract class EvDbViewBase<T> : EvDbViewBase, IEvDbView<T>
{
    protected EvDbViewBase(
        EvDbViewAddress address,
        EvDbStoredSnapshot snapshot,
        JsonSerializerOptions? options) :
            base(address, options, snapshot.Offset)
    {
        if (snapshot == EvDbStoredSnapshot.Empty)
            State = DefaultState;
        else
            State = JsonSerializer.Deserialize<T>(snapshot.State, options);
    }

    protected abstract T DefaultState { get; }

    public virtual T State { get; protected set; }
}

[DebuggerDisplay("Offset:[Stored: {StoreOffset}, Folded:{FoldOffset}], ShouldStore:[{ShouldStoreSnapshot}]")]
public abstract class EvDbViewBase :
    IEvDbView
{
    protected readonly JsonSerializerOptions? _options;

    #region Ctor

    protected EvDbViewBase(
        EvDbViewAddress address,
        JsonSerializerOptions? options,
        long storedOffset = -1)
    {
        _options = options;
        StoreOffset = storedOffset;
        FoldOffset = storedOffset;
        Address = address;
    }

    #endregion // Ctor

    #region onSaved

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

    public EvDbViewAddress Address { get; }

    public long FoldOffset { get; private set; }

    public long StoreOffset { get; set; } = -1;

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

