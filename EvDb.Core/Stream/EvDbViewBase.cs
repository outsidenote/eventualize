using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

public abstract class EvDbViewBase :
    IEvDbView
{
    protected readonly JsonSerializerOptions? _options;
    private long _lastFoldedOffset;

    protected EvDbViewBase(
        EvDbViewAddress address,
        JsonSerializerOptions? options, 
        long lastFoldedOffset = -1)
    {
        _options = options;
        _lastFoldedOffset = lastFoldedOffset;
        Address = address;
    }

    #region onSaved

    public void OnSaved()
    {
        if (ShouldStoreSnapshot)
            LatestStoredOffset = _lastFoldedOffset;
    }

    #endregion // OnSaved

    #region ShouldStoreSnapshot

    public bool ShouldStoreSnapshot
    {
        get
        {
            long numEventsSinceLatestSnapshot = _lastFoldedOffset - LatestStoredOffset;
            bool result = numEventsSinceLatestSnapshot >= MinEventsBetweenSnapshots;
            return result;
        }
    }

    #endregion // ShouldStoreSnapshot

    public EvDbViewAddress Address { get; }

    public long LatestStoredOffset { get; set; } = -1;

    public virtual int MinEventsBetweenSnapshots => 0;

    public void FoldEvent(IEvDbEvent e)
    {
        long offset = e.StreamCursor.Offset;
        if (_lastFoldedOffset >= offset)
            return;
        OnFoldEvent(e);
        _lastFoldedOffset = offset;
    }

    protected abstract void OnFoldEvent(IEvDbEvent e);
}

