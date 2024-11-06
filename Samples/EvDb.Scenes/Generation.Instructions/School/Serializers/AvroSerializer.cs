// Ignore Spelling: TopicProducer Topic

using EvDb.Core;

namespace EvDb.UnitTests;

internal class AvroSerializer : IEvDbOutboxSerializer
{
    (byte[] buffer, bool isHandeld) IEvDbOutboxSerializer.Apply<T>(string channel,
                                                                   EvDbTableName tableName,
                                                                   T payload)
    {
        return ([], false);
    }
}
