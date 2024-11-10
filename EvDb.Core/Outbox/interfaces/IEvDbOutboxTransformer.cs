namespace EvDb.Core;

public interface IEvDbOutboxTransformer
{
    /// <summary>
    /// Serializes the message payload to a different format (Ex: avro, protobuf, etc)
    /// </summary>
    /// <param name="channel">Logical routing channel</param>
    /// <param name="messageType">Represents the schema of the message</param>
    /// <param name="originalEventType">Event that triggered the message creation</param>
    /// <param name="payload">Payload of the message</param>
    /// <returns></returns>
    byte[] Transform(string channel,string messageType, string originalEventType, byte[] payload);
}