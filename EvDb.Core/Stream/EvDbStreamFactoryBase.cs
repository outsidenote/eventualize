using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{PartitionAddress.Domain}:{PartitionAddress.Partition}")]
public abstract class EvDbStreamFactoryBase<T> : IEvDbStreamFactory<T>
    where T : IEvDbStreamStore, IEvDbEventAdder
{
    protected readonly IEvDbStorageStreamAdapter _storageAdapter;
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    #region Ctor

    protected EvDbStreamFactoryBase(
        IEvDbStorageStreamAdapter storageAdapter,
        TimeProvider? timeProvider = null)
    {
        TimeProvider = timeProvider ?? TimeProvider.System;
        _storageAdapter = storageAdapter;
    }

    #endregion // Ctor

    public abstract EvDbPartitionAddress PartitionAddress { get; }

    public virtual int MinEventsBetweenSnapshots { get; }

    public virtual JsonSerializerOptions? Options { get; }

    #region TimeProvider

    public TimeProvider TimeProvider { get; }

    #endregion // TimeProvider

    #region Create

    public T Create(string streamId)
    {
        var address = new EvDbStreamAddress(PartitionAddress, streamId);
        var views = CreateEmptyViews(address);

        OtelTags tags = address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.Factory.CreateAsync");

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

        OtelTags tags = address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.Factory.GetAsync");
        using var duration = _sysMeters.MeasureFactoryGetDuration(tags);

        using var snapsActivity = _trace.StartActivity(tags, "EvDb.Factory.GetSnapshots");

        long minSnapshotOffset = -1;
        var tasks = ViewFactories.Select(viewFactory => GetViewAsync(viewFactory));

        #region Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)

        async Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)
        {
            EvDbViewAddress viewAddress = new(address, viewFactory.ViewName);

            using var snapActivity = _trace.StartActivity(tags, "EvDb.Factory.GetSnapshot")
                                           ?.AddTag("evdb.view.name", viewAddress.ViewName);
            IEvDbStorageSnapshotAdapter snapshotAdapter = viewFactory.StoreAdapter;
            EvDbStoredSnapshot snapshot = await snapshotAdapter.GetSnapshotAsync(viewAddress, cancellationToken);
            minSnapshotOffset = minSnapshotOffset == -1
                                    ? snapshot.Offset
                                    : Math.Min(minSnapshotOffset, snapshot.Offset);
            IEvDbViewStore view = viewFactory.CreateFromSnapshot(address, snapshot, Options);
            return view;
        }

        #endregion //  Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)

        IEvDbViewStore[] views = await Task.WhenAll(tasks);
        var immutableViews = views.ToImmutableList();

        var cursor = new EvDbStreamCursor(PartitionAddress, streamId, minSnapshotOffset + 1);
        IAsyncEnumerable<EvDbEvent> events =
            _storageAdapter.GetEventsAsync(cursor, cancellationToken);

        long streamOffset = minSnapshotOffset;
        var list = new List<EvDbEvent>();
        await foreach (EvDbEvent e in events)
        {
            list.Add(e);
        }
        foreach (var e in list)
        {
            foreach (IEvDbViewStore view in views)
            {
                view.FoldEvent(e);
            }
            streamOffset = e.StreamCursor.Offset;
        }
        T stream = OnCreate(streamId, immutableViews, streamOffset);

        return stream;
    }

    #endregion // GetEventsAsync

    #region OnCreateAsync

    protected abstract T OnCreate(
        string streamId,
        IImmutableList<IEvDbViewStore> views,
        long lastStoredEventOffset);

    #endregion // OnCreateAsync

    #region CreateEmptyViews

    protected IImmutableList<IEvDbViewStore> CreateEmptyViews(EvDbStreamAddress address)
    {
        var options = Options;
        var views = ViewFactories.Select(viewFactory => viewFactory.CreateEmpty(address, options, TimeProvider));

        var immutable = ImmutableList.CreateRange(views);
        return immutable;
    }

    #endregion // CreateEmptyViews

    #region ViewFactories

    protected abstract IEvDbViewFactory[] ViewFactories { get; }

    #endregion // ViewFactories
}

