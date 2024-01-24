using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{Kind}...")]
public abstract class EvDbFactoryBase<T> : IEvDbFactory<T>
    where T : IEvDbStreamStore, IEvDbEventAdder
{
    protected readonly IEvDbRepository _repository;

    #region Ctor

    public EvDbFactoryBase(IEvDbStorageAdapter storageAdapter)
    {
        _repository = new EvDbRepository(storageAdapter);
    }

    #endregion // Ctor

    public abstract EvDbPartitionAddress PartitionAddress { get; }

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

