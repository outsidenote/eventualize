// Ignore Spelling: OutboxHandler

using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbOutboxHandler
{
    void OnOutboxHandler(
            EvDbEvent e,
            IImmutableList<IEvDbViewStore> views);
}
