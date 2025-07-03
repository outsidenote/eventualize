// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;

using Amazon.SQS;
using Cocona;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.Sinks;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

// TODO: [Bnaya 2025-06-30] Use [Theory] instead of the inheritance for Fifo or not and Sink Channel (SNS/SQS: SQSMessageFormat), 

//[Collection("sink")] 
public abstract class AwsSinkTheoryBaseTests : BaseIntegrationTests
{
    private readonly EvDbShardName SHARD = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
    private static readonly TimeSpan DEFAULT_SQS_VISIBILITY_TIMEOUTON = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan CANCELLATION_DUCRAION = Debugger.IsAttached
                                ? TimeSpan.FromMinutes(10)
                                : TimeSpan.FromSeconds(10);
    private readonly Guid ID = Guid.NewGuid();

    #region Ctor

    protected AwsSinkTheoryBaseTests(ITestOutputHelper output,
                                     StoreType storeType) :
        base(output, storeType, true)
    {
    }

    #endregion //  Ctor

    #region InitAsync

    private async Task<(IEvDbMessagesSinkProcessor, IEvDbNoViews)> InitAsync(
                            SQSMessageFormat messageFormat,
                            string topicName,
                            string queueName)
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

        var processorRefistration = services.AddEvDb()
                 .AddSink()
                 .ForMessages()
                     .AddShard(SHARD)
                     .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2)))
                     .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
                     .BuildProcessor();


        if (messageFormat == SQSMessageFormat.SNSWrapper)
        {
            processorRefistration.SendToSNS(topicName);
            services.AddSingleton(AWSProviderFactory.CreateSNSClient());
        }
        else
        {
            processorRefistration.SendToSQS(queueName);
        }
        services.AddSingleton(AWSProviderFactory.CreateSQSClient());

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        var streamId = Guid.NewGuid();
        IEvDbNoViews stream = factory.Create(streamId);
        var sinkProcessor = sp.GetRequiredService<IEvDbMessagesSinkProcessor>();

        return (sinkProcessor, stream);
    }

    #endregion //  InitAsync

    #region SinkMessages_Succeed

    [Theory]
    [InlineData(SQSMessageFormat.SNSWrapper, true)]
    [InlineData(SQSMessageFormat.SNSWrapper, false)]
    [InlineData(SQSMessageFormat.Raw, true)]
    [InlineData(SQSMessageFormat.Raw, false)]
    public virtual async Task SinkMessages_Succeed(
                            SQSMessageFormat messageFormat,
                            bool isFifo)
    {
        string ext = isFifo ? ".fifo" : "";
        string TOPIC_NAME = $"SNS_TEST_{ID:N}{ext}";
        string QUEUE_NAME = $"SQS_TEST_{ID:N}{ext}";

        var (sinkProcessor, stream) = await InitAsync(messageFormat, TOPIC_NAME, QUEUE_NAME);
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;

        AmazonSQSClient sqsClient = AWSProviderFactory.CreateSQSClient();

        using var cts = new CancellationTokenSource(CANCELLATION_DUCRAION);
        var cancellationToken = cts.Token;

        await SubscribeSQSToSNSWhenNeededAsync(sqsClient, messageFormat, TOPIC_NAME, QUEUE_NAME, cancellationToken);
        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = sinkProcessor.StartMessagesSinkAsync(cancellationToken);

        // Start a background task to poll SQS for messages
        var sqsListeningTask = ListenToSQSAsync(sqsClient, QUEUE_NAME, count, messageFormat, cancellationToken);

        // produce messages before start listening to the change stream
        #region  await ProcuceStudentReceivedGradeAsync(...)

        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(stream, CHUNCK_SIZE, i);
        }

        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

        EvDbMessageRecord[] messages = await sqsListeningTask; // Wait for SQS listening task to complete

        await cts.CancelAsync();

        Assert.Equal(count, messages.Length);
    }

    #endregion //  SinkMessages_Succeed

    #region SinkMessages_Dataflow_Succeed

    [Theory]
    [InlineData(SQSMessageFormat.SNSWrapper, false)]
    [InlineData(SQSMessageFormat.SNSWrapper, true)]
    [InlineData(SQSMessageFormat.Raw, true)]
    [InlineData(SQSMessageFormat.Raw, false)]
    public virtual async Task SinkMessages_Dataflow_Succeed(
                            SQSMessageFormat messageFormat,
                            bool isFifo)
    {
        string ext = isFifo ? ".fifo" : "";
        string TOPIC_NAME = $"SNS_TEST_{ID:N}{ext}";
        string QUEUE_NAME = $"SQS_TEST_{ID:N}{ext}";

        var (sinkProcessor, stream) = await InitAsync(messageFormat, TOPIC_NAME, QUEUE_NAME);
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;

        AmazonSQSClient sqsClient = AWSProviderFactory.CreateSQSClient();

        using var cts = new CancellationTokenSource(CANCELLATION_DUCRAION);
        var cancellationToken = cts.Token;

        await SubscribeSQSToSNSWhenNeededAsync(sqsClient, messageFormat, TOPIC_NAME, QUEUE_NAME, cancellationToken);
        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = sinkProcessor.StartMessagesSinkAsync(cancellationToken);

        // Start a background task to poll SQS for messages
        var sqsListeningTask = ListenToSQSViaDataFlowAsync(sqsClient, QUEUE_NAME, count, messageFormat, cancellationToken);

        // produce messages before start listening to the change stream
        #region  await ProcuceStudentReceivedGradeAsync(...)

        for (int i = 0; i < count; i += CHUNCK_SIZE)
        {
            await ProcuceStudentReceivedGradeAsync(stream, CHUNCK_SIZE, i);
        }

        #endregion //   await ProcuceStudentReceivedGradeAsync(...)

        EvDbMessageRecord[] messages = await sqsListeningTask; // Wait for SQS listening task to complete

        await cts.CancelAsync();

        Assert.Equal(count, messages.Length);
    }

    #endregion //  SinkMessages_Dataflow_Succeed

    #region SinkMessages_WithTelemetry_Succeed

    [Theory]
    [InlineData(SQSMessageFormat.SNSWrapper, false)]
    [InlineData(SQSMessageFormat.SNSWrapper, true)]
    [InlineData(SQSMessageFormat.Raw, true)]
    [InlineData(SQSMessageFormat.Raw, false)]
    public virtual async Task SinkMessages_WithTelemetry_Succeed(
                            SQSMessageFormat messageFormat,
                            bool isFifo)
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

        await SinkMessages_Succeed(messageFormat, isFifo);
        Assert.Contains(traces, m => m.StartsWith("Started: EvDb.PublishTo"));
    }

    #endregion //  SinkMessages_WithTelemetry_Succeed

    #region ListenToSQSAsync

    private static async Task<EvDbMessageRecord[]> ListenToSQSAsync(AmazonSQSClient sqsClient,
                                                                    string ququeName,
                                                                    int limit,
                                                                    SQSMessageFormat messageFormat,
                                                                    CancellationToken cancellationToken)
    {
        var receivedSqsMessages = new List<EvDbMessageRecord>();
        using var cts = new CancellationTokenSource();
        using var joinCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);

        var builder = await sqsClient.CreateSQSReceiveBuilder(messageFormat)
                                .WithRequestAsync(ququeName,
                                                  o =>
                                                  {
                                                      o.MaxNumberOfMessages = 10;
                                                      o.WaitTimeSeconds = 1;
                                                  }, cancellationToken);
        var receiveResponse = builder.WithLogger(A.Fake<ILogger>())
                                     .StartAsync(joinCancellationSource.Token);

        int count = 0;


        await foreach (EvDbSQSMessageRecord msg in receiveResponse)
        {
            receivedSqsMessages.Add(msg);
            // Optionally delete the message after processing
            await sqsClient.DeleteMessageAsync(msg.SQSQueueUrl, msg.SQSReceiptHandle, cancellationToken);
            if (++count >= limit)
                break;
        }
        await joinCancellationSource.CancelAsync();

        // Short delay to avoid tight loop
        return receivedSqsMessages.ToArray();
    }

    private static async Task<EvDbMessageRecord[]> ListenToSQSViaDataFlowAsync(AmazonSQSClient sqsClient,
                                                                    string ququeName,
                                                                    int limit,
                                                                    SQSMessageFormat messageFormat,
                                                                    CancellationToken cancellationToken)
    {
        var receivedSqsMessages = new ConcurrentQueue<EvDbMessageRecord>();
        using var cts = new CancellationTokenSource();
        int count = 0;
        ActionBlock<EvDbSQSMessageRecord> block = new ActionBlock<EvDbSQSMessageRecord>(async msg =>
        {
            receivedSqsMessages.Enqueue(msg);
            // Optionally delete the message after processing
            await sqsClient.DeleteMessageAsync(msg.SQSQueueUrl, msg.SQSReceiptHandle, cancellationToken);
            int c = Interlocked.Increment(ref count);
            if (c >= limit)
                await cts.CancelAsync();
        }, new ExecutionDataflowBlockOptions { CancellationToken = cts.Token });

        var builder = await sqsClient.CreateSQSReceiveBuilder(messageFormat)
                         .WithRequestAsync(ququeName,
                                           o =>
                                           {
                                               o.MaxNumberOfMessages = 10;
                                               o.WaitTimeSeconds = 1;
                                           }, cancellationToken);
        using var joinCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token);
        await builder.WithLogger(A.Fake<ILogger>())
                     .StartAsync(block, joinCancellationSource.Token);

        return receivedSqsMessages.ToArray();
    }

    #endregion //  ListenToSQSAsync

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

    #region SubscribeSQSToSNSWhenNeededAsync

    private async Task SubscribeSQSToSNSWhenNeededAsync(
                        AmazonSQSClient sqsClient,
                        SQSMessageFormat messageFormat,
                        string TOPIC_NAME,
                        string QUEUE_NAME,
                        CancellationToken cancellationToken)
    {
        var snsClient = AWSProviderFactory.CreateSNSClient();
        if (messageFormat == SQSMessageFormat.Raw)
        {
            // No need to subscribe SQS to SNS for Raw format
            await sqsClient.GetOrCreateQueueAsync(QUEUE_NAME, DEFAULT_SQS_VISIBILITY_TIMEOUTON, cancellationToken: cancellationToken);
            return;
        }

        await snsClient.SubscribeSQSToSNSAsync(sqsClient, // will create if not exists
                                               TOPIC_NAME,
                                               QUEUE_NAME,
                                               o =>
                                               {
                                                   o.Logger = _logger;
                                                   o.SqsVisibilityTimeoutOnCreation = DEFAULT_SQS_VISIBILITY_TIMEOUTON;
                                               },
                                               CancellationToken.None);
        await Task.Delay(50);
    }

    #endregion //  SubscribeSQSToSNSWhenNeededAsync
}