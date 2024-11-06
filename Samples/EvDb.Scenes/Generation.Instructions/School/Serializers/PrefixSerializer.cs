// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using System.Text.Json;

namespace EvDb.UnitTests;

internal class PrefixSerializer : IEvDbOutboxSerializer
{
    (byte[] buffer, bool isHandeld) IEvDbOutboxSerializer.Apply<T>(string channel,
                                                                   EvDbTableName tableName,
                                                                   T payload)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload).ToList();
        json.Insert(0, 42);
        return (json.ToArray(), true);
    }
}