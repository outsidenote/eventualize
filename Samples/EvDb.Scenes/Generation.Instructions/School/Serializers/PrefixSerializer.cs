// Ignore Spelling: OutboxProducer Channel

using EvDb.Core;
using EvDb.Scenes;
using System.Text.Json;

namespace EvDb.UnitTests;

internal class PrefixSerializer : IEvDbOutboxSerializer
{
    string IEvDbOutboxSerializer.SerializerType { get; } = "Prefix";

    byte[] IEvDbOutboxSerializer.Serialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload).ToList();
        json.Insert(0, 42);
        return json.ToArray();
    }

    bool IEvDbOutboxSerializer.ShouldSerialize<T>(EvDbChannelName channel, EvDbShardName shardName, T payload)
    {
        return payload switch
        {
            StudentPassedMessage => true,
            StudentFailedMessage => true,
            _ => false
        };
    }
}