using System.Transactions;

namespace EvDb.Core;

// TODO: [bnaya 2025-01-22] consider none generic implementation that have pattern matching

/// <summary>
/// Specialized snapshot storage adapter 
/// </summary>
public abstract class EvDbTypedStorageStreamAdapter :
                        IEvDbTypedStorageSnapshotAdapter
{
    private readonly IEvDbStorageSnapshotAdapter _adapter;
    private readonly Predicate<EvDbViewAddress>? _canHandle;

    #region Ctor

    public EvDbTypedStorageStreamAdapter(
        IEvDbStorageSnapshotAdapter adapter,
        Predicate<EvDbViewAddress>? canHandle = null)
    {
        _adapter = adapter;
        _canHandle = canHandle;
    }

    #endregion //  Ctor

    /// <summary>
    /// Get the snapshot from the storage
    /// </summary>
    /// <param name="viewAddress"></param>
    /// <param name="metadata"></param>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    protected abstract Task<EvDbStoredSnapshotBase> OnGetSnapshotAsync(
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
                                        EvDbStoredSnapshotDataBase data,
                                        CancellationToken cancellation);

    /// <summary>
    /// Indication whether the adapter is compatible with the state type.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="address">The view address</param>
    /// <returns>
    /// true: the adapter is compatible with the state type.
    /// false: the adapter is not compatible with the state type.
    /// </returns>
    protected abstract bool CanHandle<TState>(EvDbViewAddress address);

    #region IEvDbTypedStorageSnapshotAdapter Members

    /// <summary>
    /// Indication whether the adapter is compatible with the state type.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <returns>
    /// true: the adapter is compatible with the state type.
    /// false: the adapter is not compatible with the state type.
    /// </returns>
    bool IEvDbTypedStorageSnapshotAdapter.CanHandle<TState>(EvDbViewAddress viewAddress)
    {
        if (_canHandle != null && !_canHandle(viewAddress))
            return false;

        return CanHandle<TState>(viewAddress);
    }

    async Task<EvDbStoredSnapshotBase> IEvDbTypedStorageSnapshotAdapter.GetSnapshotAsync(
        EvDbViewAddress viewAddress,
        CancellationToken cancellation)
    {
        EvDbStoredSnapshot meta = await _adapter.GetSnapshotAsync(viewAddress, cancellation);
        if (meta.Offset == 0)
            return EvDbStoredSnapshotBase.None;
        var snapshot = await OnGetSnapshotAsync(viewAddress, meta, cancellation);
        return snapshot;
    }

    async Task IEvDbTypedStorageSnapshotAdapter.StoreSnapshotAsync(
        EvDbStoredSnapshotDataBase data,
        CancellationToken cancellation)
    {
        using var tx = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        byte[] buffer = await OnStoreSnapshotAsync(data, cancellation);
        EvDbStoredSnapshotData snapshot = new(data, data.Offset, data.StoreOffset, buffer);
        await _adapter.StoreSnapshotAsync(snapshot, cancellation);

        tx.Complete();
    }

    #endregion //  IEvDbTypedStorageSnapshotAdapter Members
}

