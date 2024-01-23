using System.Collections.Immutable;
using System.Reflection;

// TODO [bnaya 2023-12-13] consider to encapsulate snapshot object with Snapshot<T> which is a wrapper of T that holds T and snapshotOffset 

namespace EvDb.Core;

//[DebuggerDisplay("")]
public class EvDbCollection : EvDbCollectionMeta, IEvDbCollection, IEvDbCollectionHidden
{
    private static readonly AssemblyName ASSEMBLY_NAME = Assembly.GetExecutingAssembly()?.GetName() ?? throw new NotSupportedException("GetExecutingAssembly");
    private static readonly string DEFAULT_CAPTURE_BY = $"{ASSEMBLY_NAME.Name}-{ASSEMBLY_NAME.Version}";
    protected readonly IEvDbRepositoryV1 _repository;

    #region Ctor

    public EvDbCollection(
        IEvDbStreamConfig factory,
        IImmutableList<IEvDbView> views,
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

