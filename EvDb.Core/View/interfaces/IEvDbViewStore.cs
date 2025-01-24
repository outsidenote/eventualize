namespace EvDb.Core;

public interface IEvDbViewStore : IEvDbView
{
    /// <summary>
    /// Indication whether the snapshot should be saved.
    /// </summary>
    bool ShouldStoreSnapshot { get; }

    void FoldEvent(EvDbEvent e);

    EvDbStoredSnapshotData GetSnapshotData();

    Task SaveAsync(CancellationToken cancellation = default);
}

public interface IEvDbViewStore<out T> : IEvDbViewStore
{
    T State { get; }
}
