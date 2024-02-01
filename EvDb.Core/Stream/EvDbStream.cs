using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Transactions;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;


[DebuggerDisplay("{StreamAddress}, Stored Offset:{StoreOffset} ,Count:{CountOfPendingEvents}")]
public class EvDbStream :
        IEvDbStreamStore,
        IEvDbStreamStoreData
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<EvDbEvent> _pendingEvents = ImmutableList<EvDbEvent>.Empty;

    protected static readonly SemaphoreSlim _dirtyLock = new SemaphoreSlim(1);
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";

    #region Ctor

    public EvDbStream(
        IEvDbStreamConfig factory,
        IImmutableList<IEvDbViewStore> views,
        IEvDbStorageAdapter storageAdapter,
        string streamId,
        long lastStoredOffset)
    {
        _views = views;
        _storageAdapter = storageAdapter;
        StreamAddress = new EvDbStreamAddress(factory.PartitionAddress, streamId);
        StoreOffset = lastStoredOffset;
        Options = factory.Options;
    }


    #endregion // Ctor

    #region Views

    protected readonly IImmutableList<IEvDbViewStore> _views;
    IEnumerable<IEvDbView> IEvDbStreamStoreData.Views => _views.Cast<IEvDbView>();

    private readonly IEvDbStorageAdapter _storageAdapter;

    #endregion // Views

    #region GetNextCursor

    private EvDbStreamCursor GetNextCursor()
    {
        if (CountOfPendingEvents == 0)
        { 
            var empty = new EvDbStreamCursor(StreamAddress, StoreOffset + 1);
            return empty;
        }
        EvDbEvent e = _pendingEvents[CountOfPendingEvents - 1];
        long offset = e.StreamCursor.Offset;
        var result = new EvDbStreamCursor(StreamAddress, offset + 1);
        return result;
    }

    #endregion // GetNextCursor

    #region AddEvent

    protected void AddEvent<T>(T payload, string? capturedBy = null)
        where T : IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        var json = JsonSerializer.Serialize(payload, Options);
        try
        {
            EvDbStreamCursor cursor = GetNextCursor();
            EvDbEvent e = new EvDbEvent(payload.EventType, DateTimeOffset.UtcNow, capturedBy, cursor, json);
            _dirtyLock.Wait(); // TODO: [bnaya 2024-01-09] re-consider the lock solution (ToImmutable?, custom object with length and state [hopefully immutable] that implement IEnumerable)
            _pendingEvents = _pendingEvents.Add(e); 

            foreach (IEvDbViewStore folding in _views)
                folding.FoldEvent(e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    #endregion // AddEvent

    #region SaveAsync

    async Task IEvDbStreamStore.SaveAsync(CancellationToken cancellation)
    {
        try
        {
            await _dirtyLock.WaitAsync();// TODO: [bnaya 2024-01-09] re-consider the lock solution
            {
                if (!this.HasPendingEvents)
                {
                    await Task.FromResult(true);
                    return;
                }

                await _storageAdapter.SaveAsync(this, cancellation);
                EvDbEvent ev = _pendingEvents[_pendingEvents.Count - 1];
                StoreOffset = ev.StreamCursor.Offset;
                _pendingEvents = ImmutableList<EvDbEvent>.Empty;
                foreach (var view in _views)
                {
                    view.OnSaved();
                }
            }
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    #endregion // SaveAsync

    #region StreamId

    public EvDbStreamAddress StreamAddress { get; init; }

    #endregion // StreamAddress

    public int CountOfPendingEvents => _pendingEvents.Count;

    #region LastStoredOffset

    public long StoreOffset { get; protected set; } = -1;

    #endregion // StoreOffset

    #region IsEmpty

    public bool HasPendingEvents => _pendingEvents.Count > 0;

    #endregion // HasPendingEvents

    public JsonSerializerOptions? Options { get; }

    IEnumerable<EvDbEvent> IEvDbStreamStoreData.Events => _pendingEvents;
}

