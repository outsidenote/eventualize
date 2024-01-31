namespace EvDb.Core;

public interface IEvDbView: IEvDbViewMetadata
{
    void FoldEvent(EvDbEvent e);

    void OnSaved();
}

public interface IEvDbView<out T> : IEvDbView
{
    T State { get; }
}
