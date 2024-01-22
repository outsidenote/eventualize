using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbCollectionHidden
{
    void SyncEvent(IEvDbStoredEvent e);

    IImmutableDictionary<string, IEvDbView> Views { get; }
}