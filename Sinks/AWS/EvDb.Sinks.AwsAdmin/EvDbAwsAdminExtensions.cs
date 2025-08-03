// Ignore Spelling: sns Aws
// Ignore Spelling: sqs

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using EvDb.Core.Adapters;
using EvDb.Sinks;
using EvDb.Sinks.AwsAdmin;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.Json;
using EvDb.Core;
using static EvDb.Sinks.EvDbSinkTelemetry;

#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using ms = Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static EvDb.Core.Internals.OtelConstants;
using System.Threading.Tasks.Dataflow;

#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.

#pragma warning disable S101 // Types should be named in PascalCase
#pragma warning disable CA1303 // Do not pass literals as localized parameters

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbAwsAdminExtensions
{
    private static readonly SemaphoreSlim _streamLock = new(1, 1);
    private static readonly SemaphoreSlim _queueLock = new(1, 1);
    private static readonly IMemoryCache _snsArnCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly IMemoryCache _sqsArnCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly TimeSpan SLIDING_CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

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
                                                           EvDbSinkTarget topicName,
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
                                                           EvDbSinkTarget topicName,
                                                           ms.ILogger? logger = null,
                                                           CancellationToken cancellationToken = default)
    {

        if (_snsArnCache.TryGetValue(topicName, out string? cachedTopicArn))
        {
            logger?.LogSNSTopicExists(topicName);
            return cachedTopicArn!;
        }

        await _streamLock.WaitAsync(6000, cancellationToken);
        try
        {
            // Double-check the cache to prevent a race condition.
            if (_snsArnCache.TryGetValue(topicName, out cachedTopicArn))
            {
                logger?.LogSNSTopicExists(topicName);
                return cachedTopicArn!;
            }

            string name = topicName.Value;
            string? topicArn = null;
            try
            {
                bool isFifo = name.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase);
                var attributes = new Dictionary<string, string>();
                if (isFifo)
                {
                    attributes.Add("FifoTopic", "true");
                    attributes.Add("ContentBasedDeduplication", "true"); // auto deduplication
                }
                var options = new CreateTopicRequest
                {
                    Name = name,
                    Attributes = attributes
                };

                CreateTopicResponse createTopicResponse = await snsClient.CreateTopicAsync(options, cancellationToken);

                #region Validation

                if (createTopicResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InvalidOperationException($"Failed to create or get SNS topic: {name}");
                }

                #endregion //  Validation

                topicArn = createTopicResponse.TopicArn;
                logger?.LogSNSTopicCreated(topicName);
            }
            catch (Amazon.SimpleNotificationService.Model.AuthorizationErrorException ex)
            {
                logger?.LogFailSNSNoCreateTopicPermissionAsync(name, ex);
                throw;
            }


            if (!string.IsNullOrEmpty(topicArn))
            {
                _snsArnCache.Set(topicName, topicArn, new MemoryCacheEntryOptions { SlidingExpiration = SLIDING_CACHE_EXPIRATION });
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
    public static async Task<string> GetOrCreateQueueAsync(this IAmazonSQS sqsClient,
                                                              EvDbSinkTarget queueName,
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
    public static async Task<string> GetOrCreateQueueAsync(this IAmazonSQS sqsClient,
                                                              EvDbSinkTarget queueName,
                                                              TimeSpan visibilityTimeout,
                                                              ms.ILogger? logger,
                                                              CancellationToken cancellationToken = default)
    {
        await _queueLock.WaitAsync(6000, cancellationToken);
        try
        {
            string? queueUrl = await sqsClient.TryGetQueueAsync(queueName, visibilityTimeout, logger, cancellationToken);

            if (queueUrl is not null)
            {
                logger?.LogSQSQueueExists(queueUrl);
            }
            else
            {
                bool isFifo = queueName.Value.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase);
                var attributes = new Dictionary<string, string>();
                if (isFifo)
                {
                    attributes.Add("FifoQueue", "true");
                    attributes.Add("ContentBasedDeduplication", "true"); // auto deduplication
                }

                /* TODO: consider
                 [QueueAttributeName.MessageRetentionPeriod] = (config.MessageRetentionPeriodDays * 24 * 60 * 60).ToString(),
                [QueueAttributeName.DelaySeconds] = config.DelaySeconds.ToString(),
                [QueueAttributeName.RedrivePolicy] = $$"""
                {
                    "deadLetterTargetArn": "{{dlqAttributes.Attributes["QueueArn"]}}",
                    "maxReceiveCount": {{config.MaxReceiveCount}}
                }
                 */

                string name = queueName.Value;
                var options = new CreateQueueRequest
                {
                    QueueName = name,
                    Attributes = attributes
                };
                CreateQueueResponse createQueueResponse = await sqsClient.CreateQueueAsync(options, cancellationToken);


                #region Validation

                if (createQueueResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new InvalidOperationException($"Failed to create SQS queue: {name}");
                }

                #endregion //  Validation

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

    #region GetQueueAsync

    #region Overloads

    /// <summary>
    /// Gets or creates an SQS queue with the specified name and visibility timeout.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueName"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetQueueAsync(this IAmazonSQS sqsClient,
                                                              EvDbSinkTarget queueName,
                                                              TimeSpan visibilityTimeout,
                                                              CancellationToken cancellationToken = default)
    {
        return await sqsClient.GetQueueAsync(queueName, visibilityTimeout, null, cancellationToken);
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
    public static async Task<string> GetQueueAsync(this IAmazonSQS sqsClient,
                                                              EvDbSinkTarget queueName,
                                                              TimeSpan visibilityTimeout,
                                                              ms.ILogger? logger = null,
                                                              CancellationToken cancellationToken = default)
    {
        string? queueUrl = await sqsClient.TryGetQueueAsync(queueName, visibilityTimeout, logger, cancellationToken);

        if (queueUrl is null)
            throw new InvalidOperationException($"SQS queue: {queueName} not found");

        logger?.LogSQSQueueExists(queueUrl);
        return queueUrl;
    }


    /// <summary>
    /// Gets or creates an SQS queue with the specified name and visibility timeout.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="queueName"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<string?> TryGetQueueAsync(this IAmazonSQS sqsClient,
                                                              EvDbSinkTarget queueName,
                                                              TimeSpan visibilityTimeout,
                                                              ms.ILogger? logger = null,
                                                              CancellationToken cancellationToken = default)
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
                                                    .FirstOrDefault(url => url.EndsWith($"{queueName}",
                                                                         StringComparison.OrdinalIgnoreCase));
        }

        if (existingQueueUrl is null)
            return null;

        queueUrl = existingQueueUrl;
        logger?.LogSQSQueueExists(queueUrl);
        return queueUrl;
    }


    #endregion //  GetQueueAsync

    #region SetQueueVisibilityAsync

    /// <summary>
    /// Sets the visibility timeout for the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="visibilityTimeout"></param>
    /// <param name="queueUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task SetQueueVisibilityAsync(this IAmazonSQS sqsClient,
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
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<string> GetQueueARNAsync(this IAmazonSQS sqsClient,
                                                      string queueUrl,
                                                      ms.ILogger? logger = null,
                                                      CancellationToken cancellationToken = default)
    {
        if (_sqsArnCache.TryGetValue(queueUrl, out string? cachedTopicArn))
        {
            logger?.LogSQSQueueExists(queueUrl);
            return cachedTopicArn!;
        }

        var attrs = await sqsClient.GetQueueAttributesAsync(new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames = new List<string> { "QueueArn" }
        }, cancellationToken);
        string arn = attrs.Attributes["QueueArn"];
        _sqsArnCache.Set(queueUrl, arn, new MemoryCacheEntryOptions { SlidingExpiration = SLIDING_CACHE_EXPIRATION });
        return arn;
    }
#pragma warning restore CA1054 // URI-like parameters should not be strings

    #endregion // GetQueueARNAsync

    #region SetSNSToSQSPolicyAsync

    /// <summary>
    /// Allow SNS to send to SQS (Policy)
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="topicARN"></param>
    /// <param name="queueURL"></param>
    /// <param name="queueARN"></param>
    /// <param name="principal">
    /// The AWS principal (account or '*') to allow publishing from SNS to SQS. Defaults to '*', which allows any principal.
    /// </param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task SetSNSToSQSPolicyAsync(this IAmazonSQS sqsClient,
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
    private static async Task AttachSQSToSNSAsync(
                            this AmazonSimpleNotificationServiceClient snsClient,
                            string topicARN,
                            string queueARN,
                            ms.ILogger? logger,
                            CancellationToken cancellationToken = default)
    {
        // Check if subscription exists, if not create it
        var snsSubscriptions = await snsClient.ListSubscriptionsByTopicAsync(topicARN, cancellationToken);

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
            try
            {
                var subscribeResponse = await snsClient.SubscribeAsync(new SubscribeRequest
                {
                    TopicArn = topicARN,
                    Protocol = "sqs",
                    Endpoint = queueARN
                }, cancellationToken);
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

    #region SubscribeSQSToSNSOptions

    public record SubscribeSQSToSNSOptions
    {
        public static readonly SubscribeSQSToSNSOptions Default = new();

        public string Principal { get; set; } = "*";
        public TimeSpan? SqsVisibilityTimeoutOnCreation { get; set; }
        public ms.ILogger? Logger { get; set; }
    }

    #endregion //  SubscribeSQSToSNSOptions

    #region SubscribeSQSToSNSAsync

    public static async Task SubscribeSQSToSNSAsync(
        this AmazonSimpleNotificationServiceClient snsClient,
        IAmazonSQS sqsClient,
        string topicName,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var options = SubscribeSQSToSNSOptions.Default;

        await SubscribeSQSToSNSAsync(
            snsClient,
            sqsClient,
            topicName,
            queueName,
            options.Principal,
            options.SqsVisibilityTimeoutOnCreation,
            options.Logger,
            cancellationToken);
    }

    public static async Task SubscribeSQSToSNSAsync(
        this AmazonSimpleNotificationServiceClient snsClient,
        IAmazonSQS sqsClient,
        string topicName,
        string queueName,
        Action<SubscribeSQSToSNSOptions> optionsBuilder,
        CancellationToken cancellationToken = default)
    {
        var options = SubscribeSQSToSNSOptions.Default;
        optionsBuilder?.Invoke(options);

        await SubscribeSQSToSNSAsync(
            snsClient,
            sqsClient,
            topicName,
            queueName,
            options.Principal,
            options.SqsVisibilityTimeoutOnCreation,
            options.Logger,
            cancellationToken);
    }

    // Original method (now private)
    private static async Task SubscribeSQSToSNSAsync(
        this AmazonSimpleNotificationServiceClient snsClient,
        IAmazonSQS sqsClient,
        string topicName,
        string queueName,
        string principal = "*",
        TimeSpan? sqsVisibilityTimeoutOnCreation = null,
        ms.ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var topicArn = await snsClient.GetOrCreateTopicAsync(topicName, cancellationToken);
        TimeSpan visibilityTimeout = sqsVisibilityTimeoutOnCreation ?? TimeSpan.FromMinutes(10);
        var queueUrl = await sqsClient.GetOrCreateQueueAsync(queueName, visibilityTimeout, cancellationToken: cancellationToken);
        var queueArn = await sqsClient.GetQueueARNAsync(queueUrl, logger, cancellationToken);

        await sqsClient.SetSNSToSQSPolicyAsync(topicArn,
                                               queueUrl,
                                               queueArn,
                                               principal,
                                               logger,
                                               cancellationToken: cancellationToken);

        await snsClient.AttachSQSToSNSAsync(topicArn, queueArn, logger, cancellationToken);
    }

    #endregion //  SubscribeSQSToSNSAsync

    #region SNSToMessageRecord

    /// <summary>
    /// Convert message originated via SNS and forwarded to SQS into EvDbMessageRecord.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="serializerOptions"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static EvDbMessageRecord SNSToMessageRecord(this Message message,
                                                   JsonSerializerOptions? serializerOptions = null)
    {
        try
        {
            var snsEnvelope = System.Text.Json.JsonSerializer.Deserialize<SnsNotification>(message.Body, serializerOptions);
            var eventPayload = System.Text.Json.JsonSerializer.Deserialize<EvDbMessageRecord>(snsEnvelope!.Message, serializerOptions);
            return eventPayload!;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to convert SNS message to EvDbMessageRecord.", ex);
        }
    }

    #endregion //  SNSToMessageRecord

    #region SnsNotification

    public sealed record SnsNotification
    {
        public required string Type { get; init; }
        public required string MessageId { get; init; }
        public required string TopicArn { get; init; }
        public required string Message { get; init; } // this contains your real message as JSON string
        public required DateTime Timestamp { get; init; }
    }

    #endregion //  SnsNotification

    #region ReceiveEvDbMessageRecordsAsync

    #region Overloads

    /// <summary>
    /// Receives EvDbMessageRecords from the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="block">Dataflow block</param>
    /// <param name="receiveRequest"></param>
    /// <param name="messageFormat"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async static Task ReceiveEvDbMessageRecordsAsync(this IAmazonSQS sqsClient,
                                                                    ITargetBlock<EvDbSQSMessageRecord> block,
                                                                    ReceiveMessageRequest receiveRequest,
                                                                    SQSMessageFormat messageFormat,
                                                                    ms.ILogger logger,
                                                                    CancellationToken cancellationToken = default)
    {
        await sqsClient.ReceiveEvDbMessageRecordsAsync(block, receiveRequest, messageFormat, null, logger, cancellationToken);
    }

    /// <summary>
    /// Receives EvDbMessageRecords from the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="block">Dataflow block</param>
    /// <param name="receiveRequest"></param>
    /// <param name="messageFormat"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async static Task ReceiveEvDbMessageRecordsAsync(this IAmazonSQS sqsClient,
                                                                    ITargetBlock<EvDbSQSMessageRecord> block,
                                                                    ReceiveMessageRequest receiveRequest,
                                                                    SQSMessageFormat messageFormat,
                                                                    JsonSerializerOptions? serializerOptions,
                                                                    ms.ILogger? logger,
                                                                    CancellationToken cancellationToken = default)
    {
        var stream = sqsClient.ReceiveEvDbMessageRecordsAsync(receiveRequest, messageFormat, serializerOptions, logger, cancellationToken);
        try
        {
            await foreach (var item in stream)
            {
                await block.SendAsync(item, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // this is fine
        }
        block.Complete();
    }

    /// <summary>
    /// Receives EvDbMessageRecords from the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="receiveRequest"></param>
    /// <param name="messageFormat"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static IAsyncEnumerable<EvDbSQSMessageRecord> ReceiveEvDbMessageRecordsAsync(this IAmazonSQS sqsClient,
                                                                    ReceiveMessageRequest receiveRequest,
                                                                    SQSMessageFormat messageFormat,
                                                                    ms.ILogger logger,
                                                                    CancellationToken cancellationToken = default)
    {
        return ReceiveEvDbMessageRecordsAsync(sqsClient, receiveRequest, messageFormat, null, logger, cancellationToken);
    }

    #endregion //  Overloads

    /// <summary>
    /// Receives EvDbMessageRecords from the specified SQS queue.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="receiveRequest"></param>
    /// <param name="messageFormat"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async static IAsyncEnumerable<EvDbSQSMessageRecord> ReceiveEvDbMessageRecordsAsync(this IAmazonSQS sqsClient,
                                                                    ReceiveMessageRequest receiveRequest,
                                                                    SQSMessageFormat messageFormat,
                                                                    JsonSerializerOptions? serializerOptions,
                                                                    ms.ILogger? logger,
                                                                    [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ReceiveMessageResponse receiveResponse =
                            await sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);
            foreach (Message msg in receiveResponse.Messages ?? [])
            {
                EvDbMessageRecord message = messageFormat switch
                {
                    SQSMessageFormat.SNSWrapper => msg.SNSToMessageRecord(serializerOptions),
                    _ => System.Text.Json.JsonSerializer.Deserialize<EvDbMessageRecord>(msg.Body, serializerOptions)
                };

                var parentContext = message.TelemetryContext.ToTelemetryContext();
                using Activity? activity = OtelSinkTrace.CreateBuilder("EvDb.ReceivedFromSQS")
                    .WithParent(parentContext, OtelParentRelation.Link)
                    .WithKind(ActivityKind.Consumer)
                                          .AddTags(message.ToTelemetryTags())
                                          .AddTag(TAG_SINK_TARGET_NAME, receiveRequest.QueueUrl)
                                          .AddTag(TAG_SINK_MESSAGE_ID_NAME, msg.MessageId)
                                          .AddTag(TAG_STORAGE_TYPE_NAME, "SQS")
                                          .Start();
                logger?.LogReceivedFromSQS(receiveRequest.QueueUrl, msg.MessageId, message.Id, message.EventType, message.StreamId, message.Offset, message.MessageType, message.Channel);

                EvDbSQSMessageRecord item = new EvDbSQSMessageRecord(receiveRequest.QueueUrl, message, msg);
                yield return item;
            }
        }
    }

    #endregion //  ReceiveEvDbMessageRecordsAsync

    #region CreateSQSReceiveBuilder

    /// <summary>
    /// Create a builder for receiving messages from SQS.
    /// </summary>
    /// <param name="sqsClient"></param>
    /// <param name="messageFormat"></param>
    /// <returns></returns>
    public static SQSReceiveBuilderInit CreateSQSReceiveBuilder(this IAmazonSQS sqsClient, SQSMessageFormat messageFormat)
        => new SQSReceiveBuilderInit(sqsClient, messageFormat);

    #endregion //  CreateSQSReceiveBuilder

    #region SQSReceiveBuilder

    /// <summary>
    /// Builder for receiving messages from SQS.
    /// </summary>
    public readonly record struct SQSReceiveBuilderInit
    {
        private readonly IAmazonSQS _sqsClient;
        private readonly SQSMessageFormat _messageFormat;

        public SQSReceiveBuilderInit(IAmazonSQS sqsClient, SQSMessageFormat messageFormat)
        {
            _sqsClient = sqsClient;
            _messageFormat = messageFormat;
        }

        #region WithRequest

        /// <summary>
        /// Set the request options for receiving messages from SQS.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SQSReceiveBuilder WithRequest(ReceiveMessageRequest request)
        {
            return new SQSReceiveBuilder(_sqsClient, _messageFormat, request);
        }

        #endregion //  WithRequest

        #region WithRequestAsync

        #region overloads

        /// <summary>
        /// Translate the queue name to the SQS receive request.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SQSReceiveBuilder> WithRequestAsync(string queueName,
                                                              CancellationToken cancellationToken = default)
        {
            return await WithRequestAsync(queueName, null, cancellationToken);
        }

        #endregion //  overloads

        /// <summary>
        /// Translate the queue name to the SQS receive request.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SQSReceiveBuilder> WithRequestAsync(string queueName,
                                                              Action<ReceiveMessageRequest>? options,
                                                              CancellationToken cancellationToken = default)
        {
            var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(queueName, cancellationToken);
            var queueUrl = queueUrlResponse.QueueUrl;
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 1
            };
            options?.Invoke(receiveRequest);
            return new SQSReceiveBuilder(_sqsClient, _messageFormat, receiveRequest);
        }

        #endregion //  WithRequestAsync
    }

    #endregion //  SQSReceiveBuilderInit

    #region SQSReceiveBuilder

    /// <summary>
    /// Builder for receiving messages from SQS.
    /// </summary>
    public readonly record struct SQSReceiveBuilder
    {
        public SQSReceiveBuilder(IAmazonSQS sqsClient,
                                     SQSMessageFormat messageFormat,
                                     ReceiveMessageRequest request)
        {
            SqsClient = sqsClient;
            MessageFormat = messageFormat;
            Request = request;
        }


        public IAmazonSQS SqsClient { get; init; }

        public ReceiveMessageRequest Request { get; init; }

        public SQSMessageFormat MessageFormat { get; init; }

        public JsonSerializerOptions? SerializerOptions { get; init; }

        public ms.ILogger? Logger { get; init; }

        public ITargetBlock<EvDbSQSMessageRecord>? TargetBlock { get; init; }

        #region WithLogger

        /// <summary>
        /// Attach a logger
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public SQSReceiveBuilder WithLogger(ms.ILogger logger)
        {
            return this with { Logger = logger };
        }

        #endregion //  WithLogger

        #region WithSerializerOptions

        /// <summary>
        /// Attach serialization options
        /// </summary>
        /// <param name="serializerOptions"></param>
        /// <returns></returns>
        public SQSReceiveBuilder WithSerializerOptions(JsonSerializerOptions serializerOptions)
        {
            return this with { SerializerOptions = serializerOptions };
        }

        #endregion //  WithSerializerOptions

        #region BuildAsync

        public IAsyncEnumerable<EvDbSQSMessageRecord> StartAsync(CancellationToken cancellationToken)
        {
            var stream = SqsClient.ReceiveEvDbMessageRecordsAsync(
                                                           Request,
                                                           MessageFormat,
                                                           SerializerOptions,
                                                           Logger,
                                                           cancellationToken);
            return stream;
        }

        public async Task StartAsync(ITargetBlock<EvDbSQSMessageRecord> targetBlock, CancellationToken cancellationToken)
        {
            await SqsClient.ReceiveEvDbMessageRecordsAsync(targetBlock,
                                                           Request,
                                                           MessageFormat,
                                                           SerializerOptions,
                                                           Logger,
                                                           cancellationToken);
        }

        #endregion //  BuildAsync
    }

    #endregion //  SQSReceiveBuilder
}
