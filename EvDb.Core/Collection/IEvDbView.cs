
namespace EvDb.Core;

public interface IEvDbView
{
    int MinEventsBetweenSnapshots { get; }

    void FoldEvent(IEvDbEvent e);

    string PropertyName { get; }
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
