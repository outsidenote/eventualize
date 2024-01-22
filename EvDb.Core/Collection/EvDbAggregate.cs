using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

[Obsolete("Deprecated")]
[DebuggerDisplay("LastStoredOffset: {LastStoredOffset}, State: {State}")]
public class EvDbAggregate<TState> : EvDbCollectionMeta, IEvDbAggregateDeprecated<TState>, IEvDbCollectionHidden
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
            // TODO: [bnaya 2023-12-19] thread safe
            State = FoldingLogic.FoldEvent(State, e);
        }
        finally
        {
            _dirtyLock.Release();
        }
    }

    #endregion // AddEvent

    void IEvDbCollectionHidden.SyncEvent(IEvDbStoredEvent e)
    {
        State = FoldingLogic.FoldEvent(State, e);
        LastStoredOffset = e.StreamCursor.Offset;
    }

    IImmutableDictionary<string, IEvDbView> IEvDbCollectionHidden.Views => throw new NotImplementedException();

    public void ClearLocalEvents()
    {
        LastStoredOffset += _pendingEvents.Count;
        _pendingEvents = _pendingEvents.Clear();
    }

    async Task IEvDbAggregateDeprecated<TState>.SaveAsync(CancellationToken cancellation)
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

