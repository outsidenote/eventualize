// Ignore Spelling: sns
// Ignore Spelling: sqs

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using EvDb.Sinks.AwsAdmin;
using System.Globalization;
using ms = Microsoft.Extensions.Logging;

#pragma warning disable S101 // Types should be named in PascalCase
#pragma warning disable CA1303 // Do not pass literals as localized parameters

namespace Microsoft.Extensions;

public static class EvDbAwsAdminExtensions
{
    private static readonly SemaphoreSlim _streamLock = new(1, 1);
    private static readonly SemaphoreSlim _queueLock = new(1, 1);

    #region GetOrCreateTopicAsync

    #region Overloads

    /// <summary>
    /// Gets or creates an SNS topic with the specified name.
    /// </summary>
    /// <param name="snsClient"></param>
    /// <param name="topicName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetOrCreateTopicAsync(this AmazonSimpleNotificationServiceClient snsClient,
                                                           string topicName,
                                                           CancellationToken cancellationToken = default)
    {
        return await snsClient.GetOrCreateTopicAsync(topicName, null, cancellationToken);
    }

    #endregion //  Overloads

    /// <summary>
    /// Gets or creates an SNS topic with the specified name.
    /// </summary>
    /// <param name="snsClient"></param>
    /// <param name="topicName"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetOrCreateTopicAsync(this AmazonSimpleNotificationServiceClient snsClient,
                                                           string topicName,
                                                           ms.ILogger? logger = null,
                                                           CancellationToken cancellationToken = default)
    {
        await _streamLock.WaitAsync(6000);
        try
        {
            var listTopicsResponse = await snsClient.ListTopicsAsync(cancellationToken);
            string? topicArn = listTopicsResponse.Topics switch
            {
                null => null,
                { Count: > 0 } => listTopicsResponse.Topics[0].TopicArn,
                _ => null
            };


            if (string.IsNullOrEmpty(topicArn))
            {
                var createTopicResponse = await snsClient.CreateTopicAsync(topicName, cancellationToken);
                topicArn = createTopicResponse.TopicArn;
                logger?.LogSNSTopicCreated(topicName);
            }
            else
            {
                logger?.LogSNSTopicExists(topicName);
                Console.WriteLine($"Using existing SNS topic: {topicArn}");
            }

            return topicArn;
        }
        finally
        {
            _streamLock.Release();
        }
    }

    #endregion //  GetOrCreateTopicAsync

    #region GetOrCreateQueueAsync

    #region Overloads

    /// <summary>
    /// Gets or creates an SQS queue with the specified name and visibility timeout.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueName"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetOrCreateQueueAsync(this AmazonSQSClient sqsClient,
                                                              string queueName,
                                                              TimeSpan visibilityTimeout,
                                                              CancellationToken cancellationToken = default)
    {
        return await sqsClient.GetOrCreateQueueAsync(queueName, visibilityTimeout, null, cancellationToken);
    }

    #endregion //  Overloads

    /// <summary>
    /// Gets or creates an SQS queue with the specified name and visibility timeout.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueName"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetOrCreateQueueAsync(this AmazonSQSClient sqsClient,
                                                              string queueName,
                                                              TimeSpan visibilityTimeout,
                                                              ms.ILogger? logger = null,
                                                              CancellationToken cancellationToken = default)
    {
        await _queueLock.WaitAsync(6000, cancellationToken);
        try
        {
            var listQueuesResponse = await sqsClient.ListQueuesAsync(new ListQueuesRequest
            {
                QueueNamePrefix = queueName
            },
            cancellationToken);

            string queueUrl;
            string? existingQueueUrl = null;
            if (listQueuesResponse.QueueUrls is not null)
            {
                existingQueueUrl = listQueuesResponse.QueueUrls
                                                        .FirstOrDefault(url => url.EndsWith($"/{queueName}",
                                                                             StringComparison.OrdinalIgnoreCase));
            }

            if (existingQueueUrl is not null)
            {
                queueUrl = existingQueueUrl;
                logger?.LogSQSQueueExists(queueUrl);
            }
            else
            {
                var createQueueResponse = await sqsClient.CreateQueueAsync(queueName, cancellationToken);

                queueUrl = createQueueResponse.QueueUrl;
                await SetQueueVisibilityAsync(sqsClient, visibilityTimeout, queueUrl, cancellationToken);

                logger?.LogSQSQueueCreated(queueUrl);
            }
            return queueUrl;
        }
        finally
        {
            _queueLock.Release();
        }
    }

    #endregion //  GetOrCreateQueueAsync

    #region SetQueueVisibilityAsync

    /// <summary>
    /// Sets the visibility timeout for the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="queueUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task SetQueueVisibilityAsync(this AmazonSQSClient sqsClient,
                                                      TimeSpan visibilityTimeout,
                                                      string queueUrl,
                                                      CancellationToken cancellationToken = default)
    {
        string visibilityValue = ((int)visibilityTimeout.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        await sqsClient.SetQueueAttributesAsync(new SetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            Attributes = new Dictionary<string, string>
                                        {
                                            { QueueAttributeName.VisibilityTimeout, visibilityValue}
                                        }
        },
        cancellationToken);
    }

    #endregion //  SetQueueVisibilityAsync

    #region GetQueueArnAsync

#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1054 // URI-like parameters should not be strings
#pragma warning restore CA1054 // URI-like parameters should not be strings
#pragma warning disable CA1054 // URI-like parameters should not be strings

    /// <summary>
    /// Gets the ARN of the specified SQS queue asynchronously.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetQueueARNAsync(this AmazonSQSClient sqsClient,
                                                      string queueUrl,
                                                      CancellationToken cancellationToken = default)
#pragma warning restore CA1054 // URI-like parameters should not be strings
    {
        var attrs = await sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames = new List<string> { "QueueArn" }
        }, cancellationToken);
        string arn = attrs.Attributes["QueueArn"];
        return arn;
    }

    #endregion // GetQueueARNAsync

    #region SetSNSToSQSPolicyAsync

    #region Overloads

#pragma warning disable CA1054 // URI-like parameters should not be strings
    /// <summary>
    /// Allow SNS to send to SQS (Policy)
    /// </summary>
    /// <param name="sqsClient">The SQS client.</param>
    /// <param name="topicARN">The topic ARN.</param>
    /// <param name="queueURL">The queue URL.</param>
    /// <param name="queueARN">The queue ARN.</param>
    /// <returns></returns>
    public static async Task SetSNSToSQSPolicyAsync(this AmazonSQSClient sqsClient,
                                                    string topicARN,
                                                    string queueURL,
                                                    string queueARN,
                                                    CancellationToken cancellationToken = default)
    {
        await sqsClient.SetSNSToSQSPolicyAsync(topicARN, queueURL, queueARN, "*", cancellationToken);
    }

    /// <summary>
    /// Allow SNS to send to SQS (Policy)
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueURL"></param>
    /// <param name="queueARN"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SetSNSToSQSPolicyAsync(this AmazonSQSClient sqsClient,
                                                    string topicARN,
                                                    string queueURL,
                                                    string queueARN,
                                                    ms.ILogger? logger = null,
                                                    CancellationToken cancellationToken = default)
    {
        await sqsClient.SetSNSToSQSPolicyAsync(topicARN, queueURL, queueARN, "*", null, cancellationToken);
    }

    /// <summary>
    /// Allow SNS to send to SQS (Policy)
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueURL"></param>
    /// <param name="queueARN"></param>
    /// <param name="principal"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SetSNSToSQSPolicyAsync(this AmazonSQSClient sqsClient,
                                                    string topicARN,
                                                    string queueURL,
                                                    string queueARN,
                                                    string principal = "*",
                                                    CancellationToken cancellationToken = default)
    {
        await sqsClient.SetSNSToSQSPolicyAsync(topicARN, queueURL, queueARN, principal, null, cancellationToken);
    }

    #endregion //  Overloads

    /// <summary>
    /// Allow SNS to send to SQS (Policy)
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueURL"></param>
    /// <param name="queueARN"></param>
    /// <param name="principal"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task SetSNSToSQSPolicyAsync(this AmazonSQSClient sqsClient,
                                                    string topicARN,
                                                    string queueURL,
                                                    string queueARN,
                                                    string principal = "*",
                                                    ms.ILogger? logger = null,
                                                    CancellationToken cancellationToken = default)
    {
        string policy = $$"""
                        {
                            "Version":"2012-10-17",
                            "Statement":[{
                                            "Sid":"AllowSNSPublish",
                                            "Effect":"Allow",
                                            "Principal":{"AWS":"{{principal}}"},
                                            "Action":"sqs:SendMessage",
                                            "Resource":"{{queueARN}}",
                                            "Condition":{
                                                            "ArnEquals":{"aws:SourceArn":"{{topicARN}}"}
                                                        }
                                         }]
                        }
                        """;

        await sqsClient.SetQueueAttributesAsync(queueURL, new Dictionary<string, string>
        {
            { "Policy", policy }
        }, cancellationToken);

        logger?.LogSQSPolicyAttachedExists(queueURL, principal);
    }
#pragma warning restore CA1054 // URI-like parameters should not be strings

    #endregion //  SetSNSToSQSPolicyAsync

    #region AllowSNSToSendToSQSAsync
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning restore CA1062 // Validate arguments of public methods

    #region Overloads

    /// <summary>
    /// Attaches an SQS queue to an SNS topic if not already attached.
    /// </summary>
    /// <param name="snsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueARN"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task AttachSQSToSNSAsync(
                            this AmazonSimpleNotificationServiceClient snsClient,
                            string topicARN,
                            string queueARN,
                            CancellationToken cancellationToken = default)
    {
        await snsClient.AttachSQSToSNSAsync(topicARN, queueARN, null, cancellationToken);
    }

    #endregion //  Overloads

    /// <summary>
    /// Attaches an SQS queue to an SNS topic if not already attached.
    /// As result the SQS queue will receive messages published to the SNS topic.
    /// </summary>
    /// <param name="snsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueARN"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task AttachSQSToSNSAsync(
                            this AmazonSimpleNotificationServiceClient snsClient,
                            string topicARN,
                            string queueARN,
                            ms.ILogger? logger,
                            CancellationToken cancellationToken = default)
    {
        // Check if subscription exists, if not create it
        var snsSubscriptions = await snsClient.ListSubscriptionsByTopicAsync(topicARN);

        bool subscriptionExists = false;
        foreach (var sub in snsSubscriptions.Subscriptions ?? [])
        {
            Console.WriteLine($"  - {sub.Protocol}: {sub.Endpoint}");
            if (sub.Protocol == "sqs" && sub.Endpoint.Contains(queueARN, StringComparison.OrdinalIgnoreCase))
            {
                subscriptionExists = true;
            }
        }

        if (!subscriptionExists)
        {
            Console.WriteLine("Creating SNS to SQS subscription...");
            try
            {
                var subscribeResponse = await snsClient.SubscribeAsync(new SubscribeRequest
                {
                    TopicArn = topicARN,
                    Protocol = "sqs",
                    Endpoint = queueARN
                });
                logger?.LogSQSAttachedToSNSAsync(topicARN, queueARN, subscribeResponse.SubscriptionArn);
            }
            catch (Exception ex)
            {
                logger?.LogFailToAttachSQSToSNSAsync(topicARN, queueARN, ex);
            }
        }
    }
#pragma warning restore CA1031 // Do not catch general exception types

    #endregion //  AllowSNSToSendToSQSAsync
}
