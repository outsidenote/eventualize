// Ignore Spelling: OutboxProducer Channel

namespace EvDb.Core.Internals;

public interface IEvDbOutboxProducerGeneric
{
    void Add<T>(T payload, string channel, EvDbShardName shardName) where T : IEvDbPayload;
}