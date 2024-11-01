// Ignore Spelling: TopicProducer Topic

namespace EvDb.Core.Internals;

public interface IEvDbTopicProducerGeneric
{
    void Add<T>(T payload, string topic, EvDbTableName tableName) where T : IEvDbPayload;
}