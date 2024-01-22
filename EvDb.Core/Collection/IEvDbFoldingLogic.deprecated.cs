
namespace EvDb.Core;

[Obsolete("Deprecated")]
public interface IEvDbFoldingLogic<T>
{
    T FoldEvent(T oldState, IEvDbEvent e);
}
