// Ignore Spelling: OutboxProducer

using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbOutboxProducer
{
    /// <summary>
    /// Gets the state transform abstraction into the outbox.
    /// </summary>
    void OnProduceOutboxMessages(
            EvDbEvent e,
            IImmutableList<IEvDbViewStore> views);
}
