// Ignore Spelling: TopicProducer Topic

using VogenTableName;

namespace EvDb.Core.Internals;

public interface IEvDbTopicProducerGeneric
{
    void Add<T>(T payload, string topic, EvDbTableName tableName) where T : IEvDbPayload;
}