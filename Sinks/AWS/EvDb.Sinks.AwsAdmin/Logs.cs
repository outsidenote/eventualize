using Microsoft.Extensions.Logging;

namespace EvDb.Sinks.AwsAdmin;

internal static partial class Logs
{
    [LoggerMessage(LogLevel.Debug, """
                    SQS queue [{queueUrl}] created successfully.
                    """)]
    public static partial void LogSQSQueueCreated(this ILogger logger,
                                                  string queueUrl);
    [LoggerMessage(LogLevel.Debug, """
                    SQS queue [{queueUrl}] exist.
                    """)]
    public static partial void LogSQSQueueExists(this ILogger logger,
                                                  string queueUrl);
    [LoggerMessage(LogLevel.Debug, """
                    Policy attached to a SQS queue [{queueUrl}]: AllowSNSPublish to [{principal}]
                    """)]
    public static partial void LogSQSPolicyAttachedExists(this ILogger logger,
                                                  string queueUrl,
                                                  string principal);

    [LoggerMessage(LogLevel.Debug, """
                    SQS queue [{queueARN}] attached to SNS topic [{topicARN}], using subscription [{SubscriptionARN}].
                    """)]
    public static partial void LogSQSAttachedToSNSAsync(this ILogger logger,
                                                        string topicARN,
                                                        string queueARN,
                                                        string SubscriptionARN);
    [LoggerMessage(LogLevel.Error, """
                    Fail to attach SQS queue [{queueARN}] to SNS topic [{topicARN}].
                    """)]
    public static partial void LogFailToAttachSQSToSNSAsync(this ILogger logger,
                                                        string topicARN,
                                                        string queueARN,
                                                        Exception exception);

    [LoggerMessage(LogLevel.Debug, "ReceivedFromSQS | EvDB: Id:{evDbId} StreamType:{streamType}, StreamId:{streamId}, Offset:{offset}, MessageType:{messageType}, Channel:{channel} | SQS: Queue:{queue}, MessageId: {sqsId}")]
    public static partial void LogReceivedFromSQS(this ILogger logger, string queue, string sqsId, Guid evDbId, string streamType, string streamId, long offset, string messageType, string channel);

    [LoggerMessage(LogLevel.Debug, """
                    SNS topic [{topicName}] created successfully.
                    """)]
    public static partial void LogSNSTopicCreated(this ILogger logger,
                                                  string topicName);
    [LoggerMessage(LogLevel.Debug, """
                    SNS topic [{topicName}] exist.
                    """)]
    public static partial void LogSNSTopicExists(this ILogger logger,
                                                  string topicName);

    [LoggerMessage(LogLevel.Error, """
                    Lacking 'sns:CreateTopic' permission. Failed to create or get topic '{TopicName}'.
                    """)]
    public static partial void LogFailSNSNoCreateTopicPermissionAsync(this ILogger logger,
                                                            string TopicName,
                                                            Exception exception);
}
