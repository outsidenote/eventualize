// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;

namespace EvDb.UnitTests;

internal class AvroSerializer : IEvDbOutboxSerializer
{
    string IEvDbOutboxSerializer.Name { get; } = "Prefix";

    byte[] IEvDbOutboxSerializer.Serialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
    {
        throw new NotImplementedException();
    }

    bool IEvDbOutboxSerializer.ShouldSerialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
    {
        //return channel switch
        //{
        //    OutboxShards.Messaging => true,
        //    _ => false,
        //};
        return false;
    }
}
