// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;

using Amazon.SQS;
using Cocona;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.Sinks;
using EvDb.UnitTests;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using Xunit.Abstractions;


public abstract class AwsSinkBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbMessagesSinkProcessor _sinkProcessor;
    private readonly Guid _streamId;
    private static readonly string TOPIC_NAME = $"SNS_TEST_{Guid.NewGuid():N}.fifo";
    private static readonly string QUEUE_NAME = $"SQS_TEST_{Guid.NewGuid():N}.fifo";
    private static readonly string QUEUE_FROM_TOPIC_NAME = $"SNS_to_SQS_TEST_{Guid.NewGuid():N}";
    private readonly EvDbShardName SHARD = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;

    #region Ctor

    protected AwsSinkBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType, true)
    {

        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;

        // Configure logging to use xUnit output
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new XUnitLoggerProvider(_output));
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        services.AddEvDb()
                .AddNoViewsFactory(c => c.ChooseStoreAdapter(storeType, TestingStreamStore), StorageContext)
                .DefaultSnapshotConfiguration(c => c.ChooseSnapshotAdapter(storeType, TestingStreamStore, AlternativeContext));
        services.AddEvDb()
                .AddChangeStream(storeType, StorageContext);

        services.AddEvDb()
                .AddSink()
                .ForMessages()
                    .AddShard(SHARD)
                    .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2)))
                    .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
                    .BuildProcessor()
                    .SendToSNS(TOPIC_NAME)
                    .SendToSQS(QUEUE_NAME);

        services.AddSingleton(AWSProviderFactory.CreateSNSClient());
        services.AddSingleton(AWSProviderFactory.CreateSQSClient());

        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        _sinkProcessor = sp.GetRequiredService<IEvDbMessagesSinkProcessor>();
        _streamId = Guid.NewGuid();
        _stream = _factory.Create(_streamId);
    }

    #endregion //  Ctor

    #region SinkMessagesToSNS_Succeed

    [Fact]
    public virtual async Task SinkMessagesToSNS_Succeed()
    {
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        var sqsClient = AWSProviderFactory.CreateSQSClient();
        var snsClient = AWSProviderFactory.CreateSNSClient();

        await sqsClient.GetOrCreateQueueAsync(QUEUE_NAME, TimeSpan.FromMinutes(10), CancellationToken.None);
        await snsClient.SubscribeSQSToSNSAsync(sqsClient, // will create if not exists
                                               TOPIC_NAME,
                                               QUEUE_FROM_TOPIC_NAME,
                                               o =>
                                               {
                                                   o.Logger = _logger;
                                                   o.SqsVisibilityTimeoutOnCreation = TimeSpan.FromMinutes(10);
                                               },
                                               CancellationToken.None);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = _sinkProcessor.StartMessagesSinkAsync(cancellationToken);

        // Start a background task to poll SQS for messages
        var sqsListeningTask = ListenToSQSAsync(sqsClient, QUEUE_FROM_TOPIC_NAME, count, cancellationToken);

        // produce messages before start listening to the change stream
        #region  await ProcuceStudentReceivedGradeAsync(...)

        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }

        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

        EvDbMessageRecord[] messages = await sqsListeningTask; // Wait for SQS listening task to complete

        await cts.CancelAsync();

        Assert.Equal(count, messages.Length);
    }

    #endregion //  SinkMessagesToSNS_Succeed

    #region SinkMessagesToSQS_Succeed

    [Fact]
    public virtual async Task SinkMessagesToSQS_Succeed()
    {
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(5);

        var sqsClient = AWSProviderFactory.CreateSQSClient();

        await sqsClient.GetOrCreateQueueAsync(QUEUE_NAME, TimeSpan.FromMinutes(10), CancellationToken.None);

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;
        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = _sinkProcessor.StartMessagesSinkAsync(cancellationToken);

        // Start a background task to poll SQS for messages
        var sqsListeningTask = ListenToSQSAsync(sqsClient, QUEUE_NAME, count, cancellationToken);

        // produce messages before start listening to the change stream
        #region  await ProcuceStudentReceivedGradeAsync(...)

        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(CHUNCK_SIZE, i);
        }

        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

        EvDbMessageRecord[] messages = await sqsListeningTask; // Wait for SQS listening task to complete

        Assert.Equal(count, messages.Length);
        // TODO: all messages offset test
    }

    #endregion //  SinkMessagesToSQS_Succeed

    #region SinkMessagesToSQS_WithTelemetry_Succeed

    [Fact]
    public virtual async Task SinkMessagesToSQS_WithTelemetry_Succeed()
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

        await SinkMessagesToSQS_Succeed();
        Assert.Contains(traces, m => m == "Started: PublishMessageToSinkAsync");
    }

    #endregion //  SinkMessagesToSQS_WithTelemetry_Succeed

    #region ListenToSQSAsync

    private static async Task<EvDbMessageRecord[]> ListenToSQSAsync(AmazonSQSClient sqsClient,
                                                                    string ququeName,
                                                                    int count,
                                                                    CancellationToken cancellationToken)
    {
        var receivedSqsMessages = new List<EvDbMessageRecord>();
        var queueUrlResponse = await sqsClient.GetQueueUrlAsync(ququeName, cancellationToken);
        var queueUrl = queueUrlResponse.QueueUrl;
        while (receivedSqsMessages.Count < count && !cancellationToken.IsCancellationRequested)
        {
            var receiveRequest = new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 1
            };

            Amazon.SQS.Model.ReceiveMessageResponse receiveResponse =
                                await sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);
            foreach (var msg in receiveResponse.Messages ?? [])
            {
                EvDbMessageRecord message = JsonSerializer.Deserialize<EvDbMessageRecord>(msg.Body);
                receivedSqsMessages.Add(message);
                // Optionally delete the message after processing
                await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, cancellationToken);
            }

            // Short delay to avoid tight loop
        }
        return receivedSqsMessages.ToArray();
    }

    #endregion //  ListenToSQSAsync

    #region ProcuceStudentReceivedGradeAsync

    private async Task ProcuceStudentReceivedGradeAsync(int numOfGrades = 3, int seed = 0)
    {
        for (int i = 1; i <= numOfGrades; i++)
        {
            var grade = new StudentReceivedGradeEvent(i, 88, i + seed);
            await _stream.AppendAsync(grade);
        }
        await _stream.StoreAsync();
    }

    #endregion //  ProcuceStudentReceivedGradeAsync
}