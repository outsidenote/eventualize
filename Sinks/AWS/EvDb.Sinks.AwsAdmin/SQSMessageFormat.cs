namespace EvDb.Sinks;

public enum SQSMessageFormat
{
    /// <summary>
    /// Raw message format, no additional processing or wrapping.
    /// </summary>
    Raw,
    /// <summary>
    /// SNS message format, used when publishing messages to AWS SNS and having SQS subscription.
    /// </summary>
    SNSWrapper
}
