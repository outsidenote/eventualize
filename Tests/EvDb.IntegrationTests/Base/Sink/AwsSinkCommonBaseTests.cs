// Ignore Spelling: Sql Aws

namespace EvDb.Core.Tests;

using Amazon.SQS;
using Cocona;
using Docker.DotNet.Models;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.Sinks;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Xunit.Abstractions;

// TODO: [Bnaya 2025-06-30] Use [Theory] instead of the inheritance for Fifo or not and Sink Channel (SNS/SQS: SQSMessageFormat), 

public abstract class AwsSinkCommonBaseTests : BaseIntegrationTests
{
    private readonly IEvDbNoViews _stream;
    protected readonly IConfiguration _configuration;
    private readonly IEvDbNoViewsFactory _factory;
    private readonly IEvDbMessagesSinkProcessor _sinkProcessor;
    private readonly Guid _streamId;
    private readonly EvDbShardName SHARD = EvDbNoViewsOutbox.DEFAULT_SHARD_NAME;
    private readonly SQSMessageFormat _messageFormat;
    private static readonly TimeSpan DEFAULT_SQS_VISIBILITY_TIMEOUTON = TimeSpan.FromMinutes(10);

    private readonly string TOPIC_NAME;
    private readonly string QUEUE_NAME;

    #region Ctor

    protected AwsSinkCommonBaseTests(ITestOutputHelper output,
                                     StoreType storeType,
                                     SQSMessageFormat messageFormat,
                                     string topicName,
                                     string queueName) :
        base(output, storeType, true)
    {
        _messageFormat = messageFormat;
        TOPIC_NAME = topicName;
        QUEUE_NAME = queueName;

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

       var processorRefistration = services.AddEvDb()
                .AddSink()
                .ForMessages()
                    .AddShard(SHARD)
                    .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow.AddSeconds(-2)))
                    .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
                    .BuildProcessor();
                    

        if (messageFormat == SQSMessageFormat.SNSWrapper)
        {
            processorRefistration.SendToSNS(TOPIC_NAME);
            services.AddSingleton(AWSProviderFactory.CreateSNSClient());
        }
        else
        {
            processorRefistration.SendToSQS(QUEUE_NAME);
        }
        services.AddSingleton(AWSProviderFactory.CreateSQSClient());

        var sp = services.BuildServiceProvider();
        _configuration = sp.GetRequiredService<IConfiguration>();
        _factory = sp.GetRequiredService<IEvDbNoViewsFactory>();
        _sinkProcessor = sp.GetRequiredService<IEvDbMessagesSinkProcessor>();
        _streamId = Guid.NewGuid();
        _stream = _factory.Create(_streamId);
    }


    #endregion //  Ctor

    #region SubscribeSQSToSNSWhenNeededAsync

    private async Task SubscribeSQSToSNSWhenNeededAsync(AmazonSQSClient sqsClient, CancellationToken cancellationToken)
    { 
        var snsClient = AWSProviderFactory.CreateSNSClient();
        if(_messageFormat == SQSMessageFormat.Raw)
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

    #region SinkMessages_Succeed

    [Fact]
    public virtual async Task SinkMessages_Succeed()
    {
        const int BATCH_SIZE = 30;
        const int CHUNCK_SIZE = 2;
        var cancellationDucraion = Debugger.IsAttached
                                        ? TimeSpan.FromMinutes(10)
                                        : TimeSpan.FromSeconds(30);

        AmazonSQSClient sqsClient = AWSProviderFactory.CreateSQSClient();

        using var cts = new CancellationTokenSource(cancellationDucraion);
        var cancellationToken = cts.Token;

        await SubscribeSQSToSNSWhenNeededAsync(sqsClient, cancellationToken);
        int count = BATCH_SIZE * 2;

        // sink messages from outbox
        Task _ = _sinkProcessor.StartMessagesSinkAsync(cancellationToken);

        // Start a background task to poll SQS for messages
        var sqsListeningTask = ListenToSQSAsync(sqsClient, QUEUE_NAME, count, _messageFormat, cancellationToken);

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

    #endregion //  SinkMessages_Succeed

    #region SinkMessages_WithTelemetry_Succeed

    [Fact]
    public virtual async Task SinkMessages_WithTelemetry_Succeed()
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

        await SinkMessages_Succeed();
        Assert.Contains(traces, m => m.StartsWith("Started: EvDb.PublishTo"));
    }

    #endregion //  SinkMessages_WithTelemetry_Succeed

    #region ListenToSQSAsync

    private static async Task<EvDbMessageRecord[]> ListenToSQSAsync(AmazonSQSClient sqsClient,
                                                                    string ququeName,
                                                                    int count,
                                                                    SQSMessageFormat messageFormat,
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

            var receiveResponse = sqsClient.ReceiveEvDbMessageRecordsAsync(receiveRequest, messageFormat, A.Fake<ILogger>(), cancellationToken);
            await foreach (var msg in receiveResponse)
            {
                receivedSqsMessages.Add(msg);
                // Optionally delete the message after processing
                await sqsClient.DeleteMessageAsync(queueUrl, msg.SQSReceiptHandle, cancellationToken);
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