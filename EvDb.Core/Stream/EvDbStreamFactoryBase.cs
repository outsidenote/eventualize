﻿using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{StreamType}")]
public abstract class EvDbStreamFactoryBase<T> : IEvDbStreamFactory<T>
    where T : IEvDbStreamStore, IEvDbEventTypes
{
    protected readonly ILogger _logger;
    protected readonly IEvDbStorageStreamAdapter _storageAdapter;
    private readonly static ActivitySource _trace = Telemetry.Trace;
    private readonly static IEvDbSysMeters _sysMeters = Telemetry.SysMeters;

    #region Ctor

    protected EvDbStreamFactoryBase(
        ILogger logger,
        IEvDbStorageStreamAdapter storageAdapter,
        TimeProvider? timeProvider = null)
    {
        TimeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger;
        _storageAdapter = storageAdapter;
    }

    #endregion // Ctor

    public abstract EvDbStreamTypeName StreamType { get; }

    public virtual JsonSerializerOptions? Options { get; }

    #region TimeProvider

    public TimeProvider TimeProvider { get; }

    #endregion // TimeProvider

    #region Create

    T IEvDbStreamFactory<T>.Create<TId>(in TId streamId)
    {
        string id = streamId.ToString()!;
        var address = new EvDbStreamAddress(StreamType, id);
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
        var address = new EvDbStreamAddress(StreamType, id);

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
            IEvDbViewStore view = await viewFactory.GetAsync(viewAddress, Options, TimeProvider, cancellationToken);
            return view;
        }

        #endregion //  Task<IEvDbViewStore> GetViewAsync(IEvDbViewFactory viewFactory)

        IEvDbViewStore[] views = await Task.WhenAll(tasks);
        T stream;
        if (views.Length == 0)
        {
            long lastOffset = await _storageAdapter.GetLastOffsetAsync(address, cancellationToken);
            stream = OnCreate(id, ImmutableList<IEvDbViewStore>.Empty, lastOffset);
        }
        else
        {
            long lowestOffset = views.Min(m => m.StoreOffset);

            ImmutableList<IEvDbViewStore> immutableViews = views.ToImmutableList();

            var cursor = new EvDbStreamCursor(address, lowestOffset + 1);
            IAsyncEnumerable<EvDbEvent> events =
                _storageAdapter.GetEventsAsync(cursor, cancellationToken);

            long streamOffset = lowestOffset;
            await foreach (EvDbEvent e in events)
            {
                foreach (IEvDbViewStore view in views)
                {
                    view.ApplyEvent(e);
                }
                streamOffset = e.StreamCursor.Offset;
            }
            stream = OnCreate(id, immutableViews, streamOffset);
        }

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

