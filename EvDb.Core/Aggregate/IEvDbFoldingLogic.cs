
namespace EvDb.Core;

public interface IEvDbFoldingLogic<T>
{
    T FoldEvent(T oldState, IEvDbEvent someEvent);
}
