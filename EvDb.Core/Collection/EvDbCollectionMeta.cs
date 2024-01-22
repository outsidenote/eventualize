using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;


[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EvDbCollectionMeta : IEvDbCollectionMeta
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<IEvDbEvent> _pendingEvents = ImmutableList<IEvDbEvent>.Empty;

    protected static readonly SemaphoreSlim _dirtyLock = new SemaphoreSlim(1);

    #region Ctor

    internal EvDbCollectionMeta(
        string kind,
        EvDbStreamAddress streamId,
        int minEventsBetweenSnapshots,
        long lastStoredOffset,
        JsonSerializerOptions? options)
    {
        Kind = kind;
        StreamId = streamId;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
        Options = options;
        SnapshotId = new(streamId, kind);
    }

    #endregion // Ctor

    // TODO: [bnaya 2023-12-11] Use Min Duration or Count between snapshots

    #region MinEventsBetweenSnapshots

    public int MinEventsBetweenSnapshots { get; init; } = 0;

    #endregion // MinEventsBetweenSnapshots

    #region StreamId

    public EvDbStreamAddress StreamId { get; init; }

    #endregion // StreamId

    public string Kind { get; }

    public int EventsCount => _pendingEvents.Count;

    #region LastStoredOffset

    public long LastStoredOffset { get; protected set; } = -1;

    #endregion // LastStoredOffset

    #region SnapshotId

    public EvDbSnapshotId SnapshotId { get; }

    #endregion // SnapshotId

    #region IsEmpty

    bool IEvDbCollectionMeta.IsEmpty => _pendingEvents.Count == 0;

    #endregion // IsEmpty

    public JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbEvent> IEvDbCollectionMeta.Events => _pendingEvents;
}

//[DebuggerDisplay("")]
public class EvDbCollection : EvDbCollectionMeta, IEvDbCollection, IEvDbCollectionHidden
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepositoryV1 _repository;

    #region Ctor

    public EvDbCollection(
        IEvDbFactory factory,
        IImmutableDictionary<string, IEvDbView> views,
        IEvDbRepositoryV1 repository,
        string streamId,
        long lastStoredOffset)
        : base(factory.Kind, new EvDbStreamAddress(factory.Partition, streamId), factory.MinEventsBetweenSnapshots, lastStoredOffset, factory.JsonSerializerOptions)
    {
        _repository = repository;
        _views = views;
    }


    #endregion // Ctor

    #region Views

    public IImmutableDictionary<string, IEvDbView> _views;

    #endregion // Views

    IImmutableDictionary<string, IEvDbView> IEvDbCollectionHidden.Views => _views;

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

            foreach (IEvDbView folding in _views.Values)
                folding.FoldEvent(e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    void IEvDbCollectionHidden.SyncEvent(IEvDbStoredEvent e)
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

    async Task IEvDbCollection.SaveAsync(CancellationToken cancellation)
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
}

