// Ignore Spelling: TopicProducer Topic

using EvDb.Core;
using System.Text.Json;

namespace EvDb.UnitTests;

internal class PrefixSerializer : IEvDbOutboxSerializer
{
    string IEvDbOutboxSerializer.Name { get; } = "Prefix";

    byte[] IEvDbOutboxSerializer.Serialize<T>(string channel, EvDbTableName tableName, T payload)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(payload).ToList();
        json.Insert(0, 42);
        return json.ToArray();
    }

    bool IEvDbOutboxSerializer.ShouldSerialize<T>(string channel, EvDbTableName tableName, T payload)
    {
        return true;
    }
}