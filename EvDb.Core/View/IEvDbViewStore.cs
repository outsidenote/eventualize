namespace EvDb.Core;

public interface IEvDbViewStore : IEvDbView
{
    void FoldEvent(EvDbEvent e);

    void OnSaved();

    EvDbStoredSnapshotAddress GetSnapshot();
    
    Task SaveAsync(CancellationToken cancellation = default);
}

public interface IEvDbViewStore<out T> : IEvDbViewStore
{
    T State { get; }
}
