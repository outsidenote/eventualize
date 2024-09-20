// Ignore Spelling: TopicProducer Topic

namespace EvDb.Core.Internals;

public interface IEvDbTopicProducerGeneric
{
    void Add<T>(T payload, string topic) where T : IEvDbPayload;
}