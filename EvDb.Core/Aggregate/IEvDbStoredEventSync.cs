namespace EvDb.Core;

public interface IEvDbStoredEventSync
{
    void SyncEvent(IEvDbStoredEvent e);
}