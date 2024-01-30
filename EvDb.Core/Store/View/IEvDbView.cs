namespace EvDb.Core;

public interface IEvDbView: IEvDbViewMetadata
{
    void FoldEvent(IEvDbEvent e);

    void OnSaved();
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
