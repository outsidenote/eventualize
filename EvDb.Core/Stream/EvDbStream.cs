using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

//[DebuggerDisplay("")]
public class EvDbStream : IEvDbStreamStore, IEvDbStreamSync
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<IEvDbEvent> _pendingEvents = ImmutableList<IEvDbEvent>.Empty;

    protected static readonly SemaphoreSlim _dirtyLock = new SemaphoreSlim(1);
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepository _repository;

    #region Ctor

    public EvDbStream(
        IEvDbStreamConfig factory,
        IImmutableList<IEvDbView> views,
        IEvDbRepository repository,
        string streamId,
        long lastStoredOffset)
    {
        _repository = repository;
        _views = views;
        StreamAddress = new EvDbStreamAddress(factory.PartitionAddress, streamId);
        LastStoredOffset = lastStoredOffset;
        Options = factory.JsonSerializerOptions;
    }


    #endregion // Ctor

    #region Views

    protected IImmutableList<IEvDbView> _views;

    #endregion // Views

    #region AddEvent

    protected void AddEvent<T>(T payload, string? capturedBy = null)
        where T : IEvDbEventPayload
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        IEvDbEvent e = EvDbEventFactory.Create(payload, capturedBy, Options);
        AddEvent(e);
    }

    private void AddEvent(IEvDbEvent e)
    {
        try
        {
            _dirtyLock.Wait(); // TODO: [bnaya 2024-01-09] re-consider the lock solution (ToImmutable?, custom object with length and state [hopefully immutable] that implement IEnumerable)
            IImmutableList<IEvDbEvent> evs = _pendingEvents.Add(e);
            _pendingEvents = evs;

            foreach (IEvDbView folding in _views)
                folding.FoldEvent(e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    void IEvDbStreamSync.SyncEvent(IEvDbStoredEvent e)
    {
        throw new NotImplementedException();
        //State = FoldingLogic.FoldEvent(State, e);
        LastStoredOffset = e.StreamCursor.Offset;
    }

    #endregion // AddEvent

    public void ClearLocalEvents()
    {
        LastStoredOffset += _pendingEvents.Count;
        _pendingEvents.Clear();
    }

    async Task IEvDbStreamStore.SaveAsync(CancellationToken cancellation)
    {
        try
        {
            await _dirtyLock.WaitAsync();// TODO: [bnaya 2024-01-09] re-consider the lock solution
            {
                await _repository.SaveAsync(this, Options, cancellation);
            }
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    #region StreamId

    public EvDbStreamAddress StreamAddress { get; init; }

    #endregion // StreamAddress

    public int EventsCount => _pendingEvents.Count;

    #region LastStoredOffset

    public long LastStoredOffset { get; protected set; } = -1;

    #endregion // LastStoredOffset

    #region IsEmpty

    bool IEvDbStreamStore.IsEmpty => _pendingEvents.Count == 0;

    #endregion // IsEmpty

    public JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbEvent> IEvDbStreamStore.Events => _pendingEvents;
}

