//// Ignore Spelling: Sql Aws

//namespace EvDb.Core.Tests;

//using Cocona;
//using Confluent.Kafka;
//using EvDb.Core.Adapters;
//using EvDb.IntegrationTests.Helpers;
//using EvDb.Scenes;
//using EvDb.Sinks;
//using EvDb.UnitTests;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using Xunit.Abstractions;

//[Trait("Sink", "Kafka")]
////[Collection("sink")] 
//public class KafkaSinkPublisherTests : BaseIntegrationTests
//{
//    private const string BOOTSTRAP_SERVERS = "localhost:9092";
//    private static readonly TimeSpan CANCELLATION_DUCRAION = Debugger.IsAttached
//                                ? TimeSpan.FromMinutes(10)
//                                : TimeSpan.FromSeconds(10);
//    private readonly Guid ID = Guid.NewGuid();

//    #region Ctor

//    protected KafkaSinkPublisherTests(ITestOutputHelper output,
//                                     StoreType storeType) :
//        base(output, storeType, true)
//    {
//    }

//    #endregion //  Ctor

//    private async Task<IEvDbTargetedMessagesSinkPublish> InitAsync(string topicName)
//    {
//        var builder = CoconaApp.CreateBuilder();
//        var services = builder.Services;

//        // Configure logging to use xUnit output
//        builder.Logging.ClearProviders();
//        builder.Logging.AddProvider(new XUnitLoggerProvider(_output));
//        builder.Logging.SetMinimumLevel(LogLevel.Debug);
       
//        #region Kafka

//        services.AddSingleton<IProducer<string, string>>(sp =>
//        {
//            var config = new ProducerConfig
//            {
//                BootstrapServers = BOOTSTRAP_SERVERS
//            };

//            var producer = new ProducerBuilder<string, string>(config)
//                //.SetValueSerializer(new EvDbMessageSerializer())
//                .Build();
//            return producer;
//        });

//        #endregion //  Kafka

//        services.AddKeyedKafkaPublishProvider(topicName);

//        var sp = services.BuildServiceProvider();
//        var publisher = sp.GetRequiredService<IEvDbTargetedMessagesSinkPublish>();

//        await KafkaHelper.CreateTopicAsync(_logger, BOOTSTRAP_SERVERS, topicName, 1, 1);

//        return publisher;
//    }

//    #region KafkaPublish_Succeed

//    [Fact]
//    public virtual async Task KafkaPublish_Succeed()
//    {
//        string TOPIC_NAME = $"Kafka_TEST_{ID:N}";

//        var publisher = await InitAsync(TOPIC_NAME);
//        using var cts = new CancellationTokenSource(CANCELLATION_DUCRAION);
//        var cancellationToken = cts.Token;

//        // TBD: Should it be EvDbMessage centric
       
//        // sink messages from outbox
//        Task _ = publisher.PublishMessageToSinkAsync( /*message?*/,cancellationToken);
//        ConsumerConfig kafkaConfig = GetKafkaConfig(TOPIC_NAME);
//        // Start a background task to poll Kafka for messages
//        var kafkaListeningTask = ListenToKafkaAsync(kafkaConfig, TOPIC_NAME, count, cancellationToken);

//        // produce messages before start listening to the change stream
//        #region  await ProcuceStudentReceivedGradeAsync(...)

//        for (int i = 0; i < count; i += CHUNCK_SIZE)
//        {
//            await ProcuceStudentReceivedGradeAsync(stream, CHUNCK_SIZE, i);
//        }

//        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

//        EvDbMessageRecord[] messages = await kafkaListeningTask; // Wait for Kafka listening task to complete

//        await cts.CancelAsync();

//        Assert.Equal(count, messages.Length);
//    }

//    #endregion //  KafkaPublish_Succeed

//    #region ListenToKafkaAsync

//    private static async Task<EvDbMessageRecord[]> ListenToKafkaAsync(
//        ConsumerConfig kafkaConfig,
//        string topic,
//        int limit,
//        CancellationToken cancellationToken)
//    {
//        var results = new ConcurrentQueue<EvDbMessageRecord>();
//        await Task.Run(() => ConsumeAsync(results), cancellationToken);
//        return results.ToArray();

//        void ConsumeAsync(ConcurrentQueue<EvDbMessageRecord> received)
//        {

//            using var consumer = new ConsumerBuilder<string, string>(kafkaConfig).Build();
//            consumer.Subscribe(topic);
            
//            try
//            {
//                while (received.Count < limit && !cancellationToken.IsCancellationRequested)
//                {
//                    var cr = consumer.Consume(cancellationToken);
//                    if (cr.IsPartitionEOF)
//                    {
//                        // End of partition reached, continue to next partition
//                        continue;
//                    }
//                    var value = cr?.Message?.Value;
//                    if (value == null)
//                    {
//                        continue; // or log and skip
//                    }
//                    var record = System.Text.Json.JsonSerializer.Deserialize<EvDbMessageRecord>(value);
//                    received.Enqueue(record);

//                    // Commit offset synchronously
//                    consumer.Commit(cr);
//                }
//            }
//            catch (OperationCanceledException) { /* Exit cleanly */ }
//            finally
//            {
//                consumer.Close();
//            }
//        }
//    }

//    #endregion //  ListenToKafkaAsync

//    #region GetKafkaConfig

//    private static ConsumerConfig GetKafkaConfig(string TOPIC_NAME)
//    {
//        ConsumerConfig kafkaConfig = new ConsumerConfig
//        {
//            Acks = Acks.All, // Ensure all messages are acknowledged
//            BootstrapServers = "localhost:9092",
//            GroupId = $"test-group-{TOPIC_NAME}",
//            AutoOffsetReset = AutoOffsetReset.Earliest,
//            EnableAutoCommit = false, // Disable auto commit to control offsets manually
//            EnablePartitionEof = true
//        };
//        return kafkaConfig;
//    }

//    #endregion //  GetKafkaConfig
//}