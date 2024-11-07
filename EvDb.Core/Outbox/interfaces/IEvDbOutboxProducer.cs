// Ignore Spelling: TopicProducer

using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbOutboxProducer
{
    /// <summary>
    /// Gets the state transform abstraction into a topic.
    /// </summary>
    void OnProduceOutboxMessages(
            EvDbEvent e,
            IImmutableList<IEvDbViewStore> views);
}
