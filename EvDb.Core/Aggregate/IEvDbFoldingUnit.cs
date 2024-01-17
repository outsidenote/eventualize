
namespace EvDb.Core;

public interface IEvDbFoldingUnit
{
    int MinEventsBetweenSnapshots { get; }

    void FoldEvent(IEvDbEvent e);
}

public interface IEvDbFoldingUnit<out T>: IEvDbFoldingUnit
{
    T State { get; }
}
