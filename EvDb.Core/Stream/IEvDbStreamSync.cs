using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbStreamSync
{
    void SyncEvent(IEvDbStoredEvent e);
}