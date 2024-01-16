
namespace EvDb.Core;

public interface IEvDbFoldingUnit
{ 
    void FoldEvent(IEvDbEvent e);
}

public interface IEvDbFoldingUnit<out T>: IEvDbFoldingUnit
{
    T State { get; }
}
