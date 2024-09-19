// Ignore Spelling: TopicProducer

using System.Collections.Immutable;

namespace EvDb.Core;

public interface IEvDbTopicProducer
{
    /// <summary>
    /// Gets the state transform abstraction into a topic.
    /// </summary>
    void OnProduceTopicMessages(
            EvDbEvent e,
            IImmutableList<IEvDbViewStore> views);
}
