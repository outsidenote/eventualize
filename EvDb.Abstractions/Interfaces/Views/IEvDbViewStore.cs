namespace EvDb.Core;

/// <summary>
/// View store contract.
/// </summary>
public interface IEvDbViewStore : IEvDbView
{
    /// <summary>
    /// Indication whether the snapshot should be saved.
    /// </summary>
    bool ShouldStoreSnapshot { get; }

    /// <summary>
    /// Apply event into the view/aggregate.
    /// Create new state based on the event.
    /// </summary>
    /// <param name="e"></param>
    void ApplyEvent(EvDbEvent e);

    /// <summary>
    /// Get the snapshot data.
    /// </summary>
    /// <returns></returns>
    EvDbStoredSnapshotData GetSnapshotData();

    /// <summary>
    /// Save a snapshot data.
    /// </summary>
    /// <param name="cancellation"></param>
    /// <returns></returns>
    Task SaveAsync(CancellationToken cancellation = default);
}

/// <summary>
/// View store contract with a generic state.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEvDbViewStore<out T> : IEvDbViewStore
{
    T State { get; }
}
