// Ignore Spelling: TopicProducer Topic

using EvDb.Core;

namespace EvDb.UnitTests;

internal class AvroSerializer : IEvDbOutboxSerializer
{
    string IEvDbOutboxSerializer.Name { get; } = "Prefix";

    byte[] IEvDbOutboxSerializer.Serialize<T>(string channel, EvDbTableName tableName, T payload)
    {
        throw new NotImplementedException();
    }

    bool IEvDbOutboxSerializer.ShouldSerialize<T>(string channel, EvDbTableName tableName, T payload)
    {
        //return channel switch
        //{
        //    OutboxTables.Messaging => true,
        //    _ => false,
        //};
       return false;
    }
}
