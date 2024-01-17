
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;


[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public abstract class EvDbAggregate : IEvDbAggregate
{
    // [bnaya 2023-12-11] Consider what is the right data type (thread safe)
    protected internal IImmutableList<IEvDbEvent> _pendingEvents = ImmutableList<IEvDbEvent>.Empty;

    protected static readonly SemaphoreSlim _dirtyLock = new SemaphoreSlim(1);

    #region Ctor

    internal EvDbAggregate(
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

    bool IEvDbAggregate.IsEmpty => _pendingEvents.Count == 0;

    #endregion // IsEmpty

    public JsonSerializerOptions? Options { get; }

    IEnumerable<IEvDbEvent> IEvDbAggregate.Events => _pendingEvents;
}

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EvDbAggregate<TState> : EvDbAggregate, IEvDbAggregate<TState>, IEvDbStoredEventSync
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepository _repository;

    #region Ctor

    public EvDbAggregate(
        IEvDbRepository repository,
        string kind,
        EvDbStreamAddress streamId,
        IEvDbFoldingLogic<TState> foldingLogic,
        int minEventsBetweenSnapshots,
        TState state,
        long lastStoredOffset,
        JsonSerializerOptions? options)
        : base(kind, streamId, minEventsBetweenSnapshots, lastStoredOffset, options)
    {
        State = state;
        _repository = repository;
        FoldingLogic = foldingLogic;
    }


    #endregion // Ctor

    #region FoldingLogic

    public readonly IEvDbFoldingLogic<TState> FoldingLogic;

    #endregion // FoldingLogic

    #region State

    public TState State { get; private set; }

    #endregion // State

    #region AddEvent

    protected void AddEvent<T>(T payload, string? capturedBy = null)
        where T: IEvDbEventPayload
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
            // TODO: [bnaya 2023-12-19] thread safe
            State = FoldingLogic.FoldEvent(State, e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    void IEvDbStoredEventSync.SyncEvent(IEvDbStoredEvent e)
    {
        State = FoldingLogic.FoldEvent(State, e);
        LastStoredOffset = e.StreamCursor.Offset;
    }

    #endregion // AddEvent

    public void ClearLocalEvents()
    {
        LastStoredOffset += _pendingEvents.Count;
        _pendingEvents = _pendingEvents.Clear();
    }

    async Task IEvDbAggregate<TState>.SaveAsync(CancellationToken cancellation)
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

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EvDbClient : EvDbAggregate, IEvDb, IEvDbStoredEventSync
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepositoryV1 _repository;

    #region Ctor

    public EvDbClient(
        IEvDbFactory factory,
        IEnumerable<IEvDbFoldingUnit> foldings,
        IEvDbRepositoryV1 repository,
        string streamId,
        long lastStoredOffset)
        : base(factory.Kind, new EvDbStreamAddress(factory.Partition, streamId), factory.MinEventsBetweenSnapshots, lastStoredOffset, factory.JsonSerializerOptions)
    {
        _repository = repository;
        Foldings = foldings;
    }


    #endregion // Ctor

    public IEnumerable<IEvDbFoldingUnit> Foldings { get; }

    #region AddEvent

    protected void AddEvent<T>(T payload, string? capturedBy = null)
        where T: IEvDbEventPayload
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

            foreach (IEvDbFoldingUnit folding in Foldings)
                folding.FoldEvent(e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    void IEvDbStoredEventSync.SyncEvent(IEvDbStoredEvent e)
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

    async Task IEvDb.SaveAsync(CancellationToken cancellation)
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

