
namespace EvDb.Core;

public interface IEvDbView
{
    int MinEventsBetweenSnapshots { get; }

    void FoldEvent(IEvDbEvent e);

    /// <summary>
    /// Gets the name of the view.
    /// </summary>
    EvDbViewAddress Address { get; }

    long LatestStoredOffset { get; set; }
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
