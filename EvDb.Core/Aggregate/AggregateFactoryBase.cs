
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace EvDb.Core;

[Obsolete("Deprecated")]
[DebuggerDisplay("{Kind}...")]
public abstract class AggregateFactoryBase<T, TState>:
        IEvDbFoldingLogic<TState>,
        IEvDbAggregateFactory<T, TState>
    where T : IEvDbAggregate<TState>, IEvDbEventTypes  
{
    protected readonly IEvDbRepository _repository;

    protected abstract TState DefaultState { get; }

    public abstract string Kind { get; }

    public abstract EvDbPartitionAddress Partition { get; }

    #region Ctor

    public AggregateFactoryBase(IEvDbStorageAdapter storageAdapter)
    {
        _repository = new EvDbRepository(storageAdapter);
    }

    #endregion // Ctor

    protected virtual int MinEventsBetweenSnapshots { get; }

    protected virtual JsonSerializerOptions? JsonSerializerOptions { get; }

    #region Create

    public abstract T Create(
        string streamId,
        long lastStoredOffset = -1);

    public abstract T Create(EvDbStoredSnapshot<TState> snapshot);

    #endregion // Create

    #region GetAsync

    async Task<T> IEvDbAggregateFactory<T, TState>.GetAsync(
        string streamId, long lastStoredOffset = -1, CancellationToken cancellationToken = default)
    {
        T agg = await _repository.GetAsync(this, streamId, cancellationToken);
        return agg;
    }

    #endregion // GetAsync

    #region FoldEvent

    TState IEvDbFoldingLogic<TState>.FoldEvent(
        TState oldState,
        IEvDbEvent e)
    {
        TState result = FoldEvent(oldState, e);
        return result;
    }
    protected abstract TState FoldEvent(
        TState oldState,
        IEvDbEvent someEvent);

    #endregion // FoldEvent
}

[DebuggerDisplay("{Kind}...")]
public abstract class EvDbFactoryBase<T>: IEvDbFactory<T>
    where T : IEvDb, IEvDbEventTypes
{
    protected readonly IEvDbRepositoryV1 _repository;

    #region Ctor

    public EvDbFactoryBase(IEvDbStorageAdapter storageAdapter)
    {
        _repository = new EvDbRepositoryV1(storageAdapter);
    }

    #endregion // Ctor

    public abstract string Kind { get; }

    public abstract EvDbPartitionAddress Partition { get; }

    public virtual int MinEventsBetweenSnapshots { get; }

    public virtual JsonSerializerOptions? JsonSerializerOptions { get; }

    #region Create

    public abstract T Create(
        string streamId,
        long lastStoredOffset = -1);

    //public abstract T Create(EvDbStoredSnapshot<TState> snapshot);

    #endregion // Create

    #region GetAsync

    async Task<T> IEvDbFactory<T>.GetAsync(
        string streamId,
        long lastStoredOffset = -1, 
        CancellationToken cancellationToken = default)
    {
        T agg = await _repository.GetAsync(this, streamId, cancellationToken);
        return agg;
    }

    #endregion // GetAsync
}

