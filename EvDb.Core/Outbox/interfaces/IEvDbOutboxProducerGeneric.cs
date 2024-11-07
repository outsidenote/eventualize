// Ignore Spelling: TopicProducer Topic

namespace EvDb.Core.Internals;

public interface IEvDbOutboxProducerGeneric
{
    void Add<T>(T payload, string channel, EvDbTableName tableName) where T : IEvDbPayload;
}