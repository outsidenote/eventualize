// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;

using Cocona;
using Confluent.Kafka;
using EvDb.Core.Adapters;
using EvDb.IntegrationTests.Helpers;
using EvDb.Scenes;
using EvDb.Sinks;
using EvDb.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Xunit.Abstractions;

[Trait("Sink", "Kafka")]
//[Collection("sink")] 
public abstract class KafkaSinkBaseTests : BaseIntegrationTests
{
    private const string BOOTSTRAP_SERVERS = "localhost:9092";
    private readonly EvDbShardName SHARD = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
    private static readonly TimeSpan CANCELLATION_DUCRAION = Debugger.IsAttached
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromSeconds(10);
    private readonly Guid ID = Guid.NewGuid();

    #region Ctor

    protected KafkaSinkBaseTests(ITestOutputHelper output,
                                     StoreType storeType) :
        base(output, storeType, true)
    {
    }

    #endregion //  Ctor

    #region InitAsync

    private async Task<(IEvDbMessagesSinkProcessor, IEvDbNoViews)> InitAsync(string topicName)
    {
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;

        // Configure logging to use xUnit output
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new XUnitLoggerProvider(_output));
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        services.AddEvDb()
                .AddNoViewsFactory(c => c.ChooseStoreAdapter(_storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(_storeType, TestingStreamStore, AlternativeContext));
        services.AddEvDb()
                .AddChangeStream(_storeType, StorageContext);

        services.AddEvDb()
                 .AddSink()
                 .ForMessages()
                     .AddShard(SHARD)
                     .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2)))
                     .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
                     .BuildProcessor()
                     .SendToKafka(topicName);

        #region Kafka

        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = BOOTSTRAP_SERVERS
            };

            var producer = new ProducerBuilder<string, string>(config)
                //.SetValueSerializer(new EvDbMessageSerializer())
                .Build();
            return producer;
        });

        #endregion //  Kafka

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        var streamId = Guid.NewGuid();
        IEvDbNoViews stream = factory.Create(streamId);
        var sinkProcessor = sp.GetRequiredService<IEvDbMessagesSinkProcessor>();

        await KafkaHelper.CreateTopicAsync(_logger, BOOTSTRAP_SERVERS, topicName, 1, 1);

        return (sinkProcessor, stream);
    }

    #endregion //  InitAsync

    #region KafkaSinkMessages_Succeed

    [Fact]
    public virtual async Task KafkaSinkMessages_Succeed()
    {
        string TOPIC_NAME = $"Kafka_TEST_{ID:N}";

        var (sinkProcessor, stream) = await InitAsync(TOPIC_NAME);
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;

        using var cts = new CancellationTokenSource(CANCELLATION_DUCRAION);
        var cancellationToken = cts.Token;

        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = sinkProcessor.StartMessagesSinkAsync(cancellationToken);
        ConsumerConfig kafkaConfig = GetKafkaConfig(TOPIC_NAME);
        // Start a background task to poll Kafka for messages
        var kafkaListeningTask = ListenToKafkaAsync(kafkaConfig, TOPIC_NAME, count, cancellationToken);

        // produce messages before start listening to the change stream
        #region  await ProcuceStudentReceivedGradeAsync(...)

        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(stream, CHUNCK_SIZE, i);
        }

        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

        EvDbMessageRecord[] messages = await kafkaListeningTask; // Wait for Kafka listening task to complete

        await cts.CancelAsync();

        Assert.Equal(count, messages.Length);
    }

    #endregion //  KafkaSinkMessages_Succeed

    #region KafkaSinkMessages_WithTelemetry_Succeed

    [Fact]
    public virtual async Task KafkaSinkMessages_WithTelemetry_Succeed()
    {
        ConcurrentQueue<string> traces = new ConcurrentQueue<string>();
        ActivityListener listener = new()
        {
            ShouldListenTo = source => source.Name == "UnitTestSource" || source.Name.StartsWith("evdb", StringComparison.OrdinalIgnoreCase),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
            ActivityStarted = activity =>
            {
                // _output.WriteLine($"Started: {activity.DisplayName}");
                traces.Enqueue($"Started: {activity.DisplayName}");
            },
            ActivityStopped = activity =>
            {
                try
                {
                    _output.WriteLine($"Stopped: {activity.DisplayName}");
                }
                catch
                {
                    // Ignore any exceptions during logging
                }
            }
        };
        ActivitySource.AddActivityListener(listener);

        using var activitySource = new ActivitySource("UnitTestSource");
        using var activity = activitySource.StartActivity("TestActivity", ActivityKind.Internal);

        await KafkaSinkMessages_Succeed();
        Assert.Contains(traces, m => m.StartsWith("Started: EvDb.PublishTo"));
    }

    #endregion //  KafkaSinkMessages_WithTelemetry_Succeed

    #region ListenToKafkaAsync

    private static async Task<EvDbMessageRecord[]> ListenToKafkaAsync(
        ConsumerConfig kafkaConfig,
        string topic,
        int limit,
        CancellationToken cancellationToken)
    {
        var results = new ConcurrentQueue<EvDbMessageRecord>();
        await Task.Run(() => ConsumeAsync(results), cancellationToken);
        return results.ToArray();

        void ConsumeAsync(ConcurrentQueue<EvDbMessageRecord> received)
        {

            using var consumer = new ConsumerBuilder<string, string>(kafkaConfig).Build();
            consumer.Subscribe(topic);

            try
            {
                while (received.Count < limit && !cancellationToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(cancellationToken);
                    if (cr.IsPartitionEOF)
                    {
                        // End of partition reached, continue to next partition
                        continue;
                    }
                    var value = cr?.Message?.Value;
                    if (value == null)
                    {
                        continue; // or log and skip
                    }
                    var record = System.Text.Json.JsonSerializer.Deserialize<EvDbMessageRecord>(value);
                    received.Enqueue(record);

                    // Commit offset synchronously
                    consumer.Commit(cr);
                }
            }
            catch (OperationCanceledException) { /* Exit cleanly */ }
            finally
            {
                consumer.Close();
            }
        }
    }

    #endregion //  ListenToKafkaAsync

    #region ProcuceStudentReceivedGradeAsync

    private async Task ProcuceStudentReceivedGradeAsync(IEvDbNoViews stream,
                                                        int numOfGrades = 3,
                                                        int seed = 0)
    {
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, 88, i + seed);
            await stream.AppendAsync(grade);
        }
        await stream.StoreAsync();
    }

    #endregion //  ProcuceStudentReceivedGradeAsync

    #region GetKafkaConfig

    private static ConsumerConfig GetKafkaConfig(string TOPIC_NAME)
    {
        ConsumerConfig kafkaConfig = new ConsumerConfig
        {
            Acks = Acks.All, // Ensure all messages are acknowledged
            BootstrapServers = "localhost:9092",
            GroupId = $"test-group-{TOPIC_NAME}",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false, // Disable auto commit to control offsets manually
            EnablePartitionEof = true
        };
        return kafkaConfig;
    }

    #endregion //  GetKafkaConfig
}