using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{PartitionAddress.Domain}:{PartitionAddress.Partition}")]
public abstract class EvDbStreamFactory<T> : IEvDbStreamFactory<T>
    where T : IEvDbStreamStore, IEvDbEventTypes
{
    protected readonly ILogger _logger;
    protected readonly IEvDbStorageStreamAdapter _storageAdapter;
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    #region Ctor

    protected EvDbStreamFactory(
        ILogger logger,
        IEvDbStorageStreamAdapter storageAdapter,
        TimeProvider? timeProvider = null)
    {
        TimeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger;
        _storageAdapter = storageAdapter;
    }

    #endregion // Ctor

    public abstract EvDbPartitionAddress PartitionAddress { get; }

    public virtual JsonSerializerOptions? Options { get; }

    #region TimeProvider

    public TimeProvider TimeProvider { get; }

    #endregion // TimeProvider

    #region Create

    T IEvDbStreamFactory<T>.Create<TId>(in TId streamId)
    {
        string id = streamId.ToString()!;
        var address = new EvDbStreamAddress(PartitionAddress, id);
        var views = CreateEmptyViews(address);

        OtelTags tags = address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.Factory.CreateAsync");

        var result = OnCreate(id, views, 0);
        return result;
    }

    #endregion // Create

    #region GetAsync

    async Task<T> IEvDbStreamFactory<T>.GetAsync<TId>(TId streamId, CancellationToken cancellationToken)
    {
        string id = streamId.ToString()!;
        var address = new EvDbStreamAddress(PartitionAddress, id);

        OtelTags tags = address.ToOtelTagsToOtelTags();
        using var activity = _trace.StartActivity(tags, "EvDb.Factory.GetAsync");
        using var duration = _sysMeters.MeasureFactoryGetDuration(tags);

        using var snapsActivity = _trace.StartActivity(tags, "EvDb.Factory.GetSnapshots");

        var tasks = ViewFactories.Select(viewFactory => GetViewAsync(viewFactory));

        #region Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)

        async Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)
        {
            EvDbViewAddress viewAddress = new(address, viewFactory.ViewName);

            using var snapActivity = _trace.StartActivity(tags, "EvDb.Factory.GetSnapshot")
                                           ?.AddTag("evdb.view.name", viewAddress.ViewName);
            IEvDbViewStore view = await viewFactory.GetAsync(viewAddress, Options, TimeProvider);
            return view;
        }

        #endregion //  Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)

        IEvDbViewStore[] views = await Task.WhenAll(tasks);
        long lowestOffset = views.Min(m => m.StoreOffset);

        var immutableViews = views.ToImmutableList();

        var cursor = new EvDbStreamCursor(PartitionAddress, id, lowestOffset + 1);
        IAsyncEnumerable<EvDbEvent> events =
            _storageAdapter.GetEventsAsync(cursor, cancellationToken);

        long streamOffset = lowestOffset;
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
        T stream = OnCreate(id, immutableViews, streamOffset);

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

