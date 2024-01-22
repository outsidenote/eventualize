
namespace EvDb.Core;

public interface IEvDbView
{
    int MinEventsBetweenSnapshots { get; }

    void FoldEvent(IEvDbEvent e);
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
