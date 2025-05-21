// Ignore Spelling: OutboxProducer Channel

namespace EvDb.Core.Internals;

/// <summary>
/// Outbox producer contract.
/// </summary>
public interface IEvDbOutboxProducerGeneric
{
    /// <summary>
    /// Append a payload to the outbox.
    /// Creates a message and put it into the outbox.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="payload"></param>
    /// <param name="channel">Represents a outbox's message tagging into semantic channel name</param>
    /// <param name="shardName">Shard name represent a the name of the outbox storage unit (table/collection)</param>
    void Append<T>(T payload, EvDbChannelName channel, EvDbShardName shardName) where T : notnull, IEvDbPayload;
}