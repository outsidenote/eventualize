using EvDb.Core;
using EvDb.Core.Adapters;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using static EvDb.DemoWebApi.DemoConstants;
using Microsoft.Extensions;
using EvDb.DemoWebApi.Outbox;
using System.Collections.Immutable;

namespace EvDb.DemoWebApi;

public class SinkJob : BackgroundService
{
    private readonly ILogger<SinkJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEvDbStorageAdmin _admin;
    private readonly State _state;

    public SinkJob(
        ILogger<SinkJob> logger,
        IServiceScopeFactory scopeFactory,
        IEvDbStorageAdmin admin,
        State state)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _admin = admin;
        _state = state;
    }

    #region CreateEnvironmentAsync

    public async Task CreateEnvironmentAsync(
        CancellationToken stoppingToken)
    {
        try
        {
            await _admin.CreateEnvironmentAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fail to create environment");
        }
    }

    #endregion //  CreateEnvironmentAsync

    #region ExecuteAsync

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await CreateEnvironmentAsync(stoppingToken);

        var sqsClient = AWSProviderFactory.CreateSQSClient();
        var snsClient = AWSProviderFactory.CreateSNSClient();

        await snsClient.SubscribeSQSToSNSAsync(sqsClient, // will create if not exists
                                       TOPIC_NAME,
                                       QUEUE_NAME,
                                       o =>
                                       {
                                           o.Logger = _logger;
                                           o.SqsVisibilityTimeoutOnCreation = TimeSpan.FromMinutes(10);
                                       },
                                       CancellationToken.None);


        var queueUrlResponse = await sqsClient.GetQueueUrlAsync(QUEUE_NAME, stoppingToken);
        var queueUrl = queueUrlResponse.QueueUrl;
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveRequest = new Amazon.SQS.Model.ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = 1
            };

            Amazon.SQS.Model.ReceiveMessageResponse receiveResponse =
                                await sqsClient.ReceiveMessageAsync(receiveRequest, stoppingToken);
            foreach (Amazon.SQS.Model.Message msg in receiveResponse.Messages ?? [])
            {
                EvDbMessageRecord message = msg.SNSToMessageRecord();
                var comments = JsonSerializer.Deserialize<CommentsMessage>(message.Payload.ToString());
                _state.Comments.AddOrUpdate(comments.Id, 
                                            ImmutableList.CreateRange(comments.Comments), 
                                            (key, oldValue) =>
                                                {
                                                    var result = oldValue.InsertRange(0, comments.Comments.Reverse());
                                                    if (result.Count > 20)
                                                    {
                                                        result = result.RemoveRange(20, result.Count - 20);
                                                    }
                                                    return result;
                                                });
                // Optionally delete the message after processing
                await sqsClient.DeleteMessageAsync(queueUrl, msg.ReceiptHandle, stoppingToken);
            }
        }

    }

    #endregion //  ExecuteAsync

}
