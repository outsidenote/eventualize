// Ignore Spelling: sns Aws
// Ignore Spelling: sqs

using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using EvDb.Sinks;
using EvDb.Sinks.EvDbSinkSNS;
using EvDb.Sinks.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions;

internal static class EvDbSNDExtensions
{
    private static readonly SemaphoreSlim _streamLock = new(1, 1);
    private static readonly TimeSpan SLIDING_CACHE_EXPIRATION = TimeSpan.FromMinutes(5);
    private static readonly IMemoryCache _snsArnCache = new MemoryCache(new MemoryCacheOptions());

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
                                                           ILogger? logger = null,
                                                           CancellationToken cancellationToken = default)
    {

        if (_snsArnCache.TryGetValue(topicName, out string? cachedTopicArn))
        {
            logger?.LogSNSTopicExists(topicName);
            return cachedTopicArn!;
        }

        await _streamLock.WaitAsync(6000);
        try
        {
            ListTopicsResponse listTopicsResponse = await snsClient.ListTopicsAsync(cancellationToken);
            List<Topic> topics = listTopicsResponse.Topics;
            string? topicArn = topics.FirstOrDefault(t =>
                                        t.TopicArn.EndsWith(topicName, StringComparison.OrdinalIgnoreCase))
                                         ?.TopicArn;


            if (string.IsNullOrEmpty(topicArn))
            {
                bool isFifo = topicName.Value.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase);
                var attributes = new Dictionary<string, string>();
                string name = topicName.Value;
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
                    throw new InvalidOperationException($"Failed to create SNS topic: {name}");
                }

                #endregion //  Validation

                topicArn = createTopicResponse.TopicArn;
                logger?.LogSNSTopicCreated(topicName);
            }
            else
            {
                logger?.LogSNSTopicExists(topicName);
                Console.WriteLine($"Using existing SNS topic: {topicArn}");
            }

            _snsArnCache.Set(topicName, topicArn, new MemoryCacheEntryOptions { SlidingExpiration = SLIDING_CACHE_EXPIRATION });

            return topicArn;
        }
        finally
        {
            _streamLock.Release();
        }
    }

    #endregion //  GetOrCreateTopicAsync
}
