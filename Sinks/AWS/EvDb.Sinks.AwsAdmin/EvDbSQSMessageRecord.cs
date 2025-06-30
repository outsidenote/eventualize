// Ignore Spelling: sqs
#pragma warning disable S101 // Types should be named in PascalCase

using Amazon.SQS.Model;

namespace EvDb.Core.Adapters;

public readonly record struct EvDbSQSMessageRecord(EvDbMessageRecord EvDbMessage, Message SQSMessage)

{
    #region static implicit operator EvDbMessageRecord(EvDbMessage e) ...

    public static implicit operator EvDbMessageRecord(EvDbSQSMessageRecord source) => source.EvDbMessage;

    public static implicit operator Message(EvDbSQSMessageRecord source) => source.SQSMessage;

    public static implicit operator EvDbMessage(EvDbSQSMessageRecord source) => source.EvDbMessage;

    #endregion //  static implicit operator EvDbMessageRecord(EvDbMessage m) ...

    /// <summary>
    /// The SQS message ID.
    /// </summary>
    public string SQSMessageId => SQSMessage.MessageId;

    /// <summary>
    /// The SQS message receipt handle.
    /// </summary>
    public string SQSReceiptHandle => SQSMessage.ReceiptHandle;

    /// <summary>
    /// The SQS message attributes.
    /// </summary>
    public EvDbMessagePayloadName EvDbPayload => EvDbMessage.Payload;
}
