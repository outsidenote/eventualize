namespace EvDb.Core;

public interface IEvDbView
{
    int MinEventsBetweenSnapshots { get; }

    void FoldEvent(IEvDbEvent e);

    /// <summary>
    /// Gets the name of the view.
    /// </summary>
    EvDbViewAddress Address { get; }

    /// <summary>
    /// The offset of the last snapshot that was stored.
    /// </summary>
    long StoreOffset { get; set; }

    bool ShouldStoreSnapshot { get; }

    void OnSaved();
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
