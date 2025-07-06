using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace EvDb.IntegrationTests.Helpers;
internal static class KafkaHelper
{
    public static async Task CreateTopicAsync(ILogger logger, string bootstrapServers, string topicName, int numPartitions = 1, short replicationFactor = 1)
    {
        var config = new AdminClientConfig { BootstrapServers = bootstrapServers };

        using var adminClient = new AdminClientBuilder(config).Build();
        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topicName,
                    NumPartitions = numPartitions,
                    ReplicationFactor = replicationFactor
                }
            });

            logger.LogInformation($"✅ Topic '{topicName}' created successfully.");
        }
        catch (CreateTopicsException e)
        {
            if (e.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                logger.LogWarning(e, $"⚠️ Topic '{topicName}' already exists.");
            else
                logger.LogError(e, $"❌ Error creating topic '{topicName}': {e.Results[0].Error.Reason}");
        }
    }
}
