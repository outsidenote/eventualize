namespace EvDb.Core;

public interface IEvDbOutboxSerializer
{
    /// <summary>
    /// Gets the serializer name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Serializes the message payload to a different format (Ex: avro, protobuf, etc)
    /// </summary>
    /// <param name="channel">Logical routing channel</param>
    /// <param name="tableName">Table name routing of the message</param>
    /// <param name="payload">Payload of the message</param>
    /// <returns></returns>
    bool ShouldSerialize<T>(string channel, EvDbTableName tableName, T payload)
        where T : IEvDbPayload;

    /// <summary>
    /// Serializes the message payload to a different format (Ex: avro, protobuf, etc)
    /// </summary>
    /// <param name="channel">Logical routing channel</param>
    /// <param name="tableName">Table name routing of the message</param>
    /// <param name="payload">Payload of the message</param>
    /// <returns></returns>
    byte[] Serialize<T>(string channel, EvDbTableName tableName, T payload)
        where T : IEvDbPayload;
}