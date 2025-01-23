using System.Transactions;

namespace EvDb.Core;

// TODO: [bnaya 2025-01-22] consider none generic implementation that have pattern matching

/// <summary>
/// Specialized snapshot storage adapter 
/// </summary>
/// <typeparam name="TState"></typeparam>
public abstract class EvDbTypedStorageStreamAdapter<TState> :
                        IEvDbStorageSnapshotAdapter<TState>
{
    private readonly IEvDbStorageSnapshotAdapter _adapter;

    #region Ctor

    public EvDbTypedStorageStreamAdapter(
        IEvDbStorageSnapshotAdapter adapter)
    {
        _adapter = adapter;
    }

    #endregion //  Ctor

    /// <summary>
    /// Get the snapshot from the storage
    /// </summary>
    /// <param name="viewAddress"></param>
    /// <param name="metadata"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected abstract Task<TState> OnGetSnapshotAsync(
                                        EvDbViewAddress viewAddress,
                                        EvDbStoredSnapshot metadata,
                                        CancellationToken cancellation);

    /// <summary>
    /// Store the snapshot in the storage
    /// </summary>
    /// <param name="data"></param>
    /// <param name="cancellation"></param>
    /// <returns>
    /// Returns the payload that will be stored as byte[].
    /// Common practice is to return the primary key of the entity,
    /// that can be use in `OnGetSnapshotAsync` to fetch the state.
    /// </returns>
    protected abstract Task<byte[]> OnStoreSnapshotAsync(
                                        EvDbStoredSnapshotData<TState> data,
                                        CancellationToken cancellation);


    #region IEvDbStorageSnapshotAdapter<TState> Members

    async Task<EvDbStoredSnapshot<TState>> IEvDbStorageSnapshotAdapter<TState>.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        EvDbStoredSnapshot meta = await _adapter.GetSnapshotAsync(viewAddress, cancellation);
        if (meta.Offset == 0)
            return new EvDbStoredSnapshot<TState>(0, default);
        TState state = await OnGetSnapshotAsync(viewAddress, meta, cancellation);
        return new EvDbStoredSnapshot<TState>(meta.Offset, state);
    }

    async Task IEvDbStorageSnapshotAdapter<TState>.StoreSnapshotAsync(
        EvDbStoredSnapshotData<TState> data,
        CancellationToken cancellation)
    {
        using var tx = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        byte[] buffer = await OnStoreSnapshotAsync(data, cancellation);
    EvDbStoredSnapshotData snapshot = new(data, data.Offset, data.StoreOffset, buffer);
        await _adapter.StoreSnapshotAsync(snapshot, cancellation);

        tx.Complete();
    }

    #endregion //  IEvDbStorageSnapshotAdapter<TState> Members
}

