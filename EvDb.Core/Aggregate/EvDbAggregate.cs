
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
        long lastStoredOffset)
    {
        Kind = kind;
        StreamId = streamId;
        MinEventsBetweenSnapshots = minEventsBetweenSnapshots;
        LastStoredOffset = lastStoredOffset;
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

    IEnumerable<IEvDbEvent> IEvDbAggregate.Events => _pendingEvents;
}

[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EvDbAggregate<TState> : EvDbAggregate, IEvDbAggregate<TState>, IEvDbStoredEventSync
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepository _repository;
    private readonly JsonSerializerOptions? _options;

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
        : base(kind, streamId, minEventsBetweenSnapshots, lastStoredOffset)
    {
        State = state;
        _options = options;
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

    protected void AddEvent(IEvDbEventPayload payload, string? capturedBy = null)
    {
        capturedBy = capturedBy ?? DEFAULT_CAPTURE_BY;
        IEvDbEvent e = EvDbEventFactory.Create(payload, capturedBy, _options);
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
        _pendingEvents.Clear();
    }

    async Task IEvDbAggregate<TState>.SaveAsync(CancellationToken cancellation)
    {
        try
        {
            await _dirtyLock.WaitAsync();// TODO: [bnaya 2024-01-09] re-consider the lock solution
            {
                await _repository.SaveAsync(this, _options, cancellation);
            }
        }
        finally
        {
            _dirtyLock.Release();
        }
    }
}

