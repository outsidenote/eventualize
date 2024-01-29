using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{Kind}...")]
public abstract class EvDbStreamFactoryBase<T> : IEvDbStreamFactory<T>
    where T : IEvDbStreamStore, IEvDbEventAdder
{
    protected readonly IEvDbStorageAdapter _storageAdapter;

    #region Ctor

    public EvDbStreamFactoryBase(IEvDbStorageAdapter storageAdapter)
    {
        _storageAdapter = storageAdapter;
    }

    #endregion // Ctor

    public abstract EvDbPartitionAddress PartitionAddress { get; }

    public virtual int MinEventsBetweenSnapshots { get; }

    public virtual JsonSerializerOptions? JsonSerializerOptions { get; }

    #region Create

    public T Create(string streamId)
    {
        var address = new EvDbStreamAddress(PartitionAddress, streamId);
        var views = CreateEmptyViews(address);

        var result = OnCreate(streamId, views, -1);
        return result;
    }

    #endregion // Create

    #region GetAsync

    async Task<T> IEvDbStreamFactory<T>.GetAsync(
        string streamId,
        CancellationToken cancellationToken)
    {
        var address = new EvDbStreamAddress(PartitionAddress, streamId);
        long minSnapshotOffset = -1; 
        long lastStoredEventOffset = -1;
        List<IEvDbView> views = new(ViewFactories.Length);
        foreach (IEvDbViewFactory viewFactory in ViewFactories)
        {
            EvDbViewAddress viewAddress = new(address, viewFactory.ViewName);
            EvDbStoredSnapshot snapshot = await _storageAdapter.TryGetSnapshotAsync(viewAddress, cancellationToken);
            minSnapshotOffset = minSnapshotOffset == -1
                                    ? snapshot.Offset + 1
                                    : Math.Min(minSnapshotOffset, snapshot.Offset + 1);
            IEvDbView view = viewFactory.CreateFromSnapshot(address, snapshot, JsonSerializerOptions);
            views.Add(view);    
        }
        var immutableViews = views.ToImmutableList();
        
        var cursor = new EvDbStreamCursor(PartitionAddress, streamId, minSnapshotOffset);
        IAsyncEnumerable<IEvDbStoredEvent> events = 
            _storageAdapter.GetAsync(cursor, cancellationToken);
        
        await foreach (IEvDbStoredEvent e in events)
        {
            foreach (IEvDbView view in views)
            {
                view.FoldEvent(e);
            }
            lastStoredEventOffset = e.StreamCursor.Offset;
        }
        T stream = OnCreate(streamId, immutableViews, lastStoredEventOffset);

        return stream;
    }

    #endregion // GetAsync

    #region OnCreateAsync

    protected abstract T OnCreate(
        string streamId,
        IImmutableList<IEvDbView> views,
        long lastStoredEventOffset);

    #endregion // OnCreateAsync

    #region CreateEmptyViews

    protected IImmutableList<IEvDbView> CreateEmptyViews(EvDbStreamAddress address)
    {
        var options = JsonSerializerOptions;
        var views = ViewFactories.Select(viewFactory => viewFactory.CreateEmpty(address, options));

        var immutable = ImmutableList.CreateRange(views);
        return immutable;
    }

    #endregion // CreateEmptyViews

    #region ViewFactories

    protected abstract IEvDbViewFactory[] ViewFactories { get; }

    #endregion // ViewFactories
}

