namespace EvDb.Core;

public interface IEvDbOutboxSerializer
{
    /// <summary>
    /// Serializes the message payload to a different format (Ex: avro, protobuf, etc)
    /// </summary>
    /// <param name="channel">Logical routing channel</param>
    /// <param name="tableName">Table name routing of the message</param>
    /// <param name="payload">Payload of the message</param>
    /// <returns></returns>
    (byte[] buffer, bool isHandeld) Apply<T>(string channel, EvDbTableName tableName, T payload)
        where T : IEvDbPayload;
}