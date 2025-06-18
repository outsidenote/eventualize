using Amazon.SQS;
using EvDb.Core;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.Sinks.EvDbSinkTelemetry;
using Microsoft.Extensions;
using EvDb.Core.Adapters;
using Amazon.SQS.Model;

#pragma warning disable S101 // Types should be named in PascalCase

namespace EvDb.Sinks.EvDbSinkSQS;

internal class EvDbSinkProviderSQS : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderSQS> _logger;
    private readonly AmazonSQSClient _client;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public EvDbSinkProviderSQS(ILogger<EvDbSinkProviderSQS> logger,
                               AmazonSQSClient client)
    {
        _logger = logger;
        _client = client;
    }

    async Task IEvDbMessagesSinkPublishProvider.PublishMessageToSinkAsync(EvDbSinkTarget target,
                                                                          EvDbMessageRecord message,
                                                                          JsonSerializerOptions? serializerOptions,
                                                                          CancellationToken cancellationToken)
    {
        ActivityContext parentContext = message.TelemetryContext.ToTelemetryContext();
        using var activity = OtelTrace.CreateBuilder()
                                      .WithParent(parentContext)
                                      .WithKind(ActivityKind.Producer)
                                      .AddTag("evdb.sink.target", target)
                                      .Start();

        _logger.LogPublish(target, message);

        string json = JsonSerializer.Serialize(message, serializerOptions);
        //string queueArn = await _client.GetQueueARNAsync(target, _logger, cancellationToken);

        // Create SNS message attributes dictionary
        var messageAttributes = new Dictionary<string, MessageAttributeValue>();

        // Get the current propagation context
        var propagationContext = new PropagationContext(
            activity?.Context ?? default,
            Baggage.Create()
        );
        Propagator.Inject(propagationContext, messageAttributes, InjectTraceContext);

        void InjectTraceContext(
                    Dictionary<string, MessageAttributeValue> carrier,
                    string key,
                    string value)
        {
            carrier[key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = value
            };
        }

        var request = new SendMessageRequest
        {
            QueueUrl = target,
            MessageBody = json,
            MessageGroupId = message.GetAddress().ToString(),
            MessageAttributes = messageAttributes,
            MessageDeduplicationId = message.Id.ToString("N"), 
        };

        await _client.SendMessageAsync(request, cancellationToken);
    }

    IEvDbTargetedMessagesSinkPublish IEvDbMessagesSinkPublishProvider.Create(EvDbSinkTarget target) => new SpecializedTarget(this, target);

    private sealed class SpecializedTarget : IEvDbTargetedMessagesSinkPublish
    {
        private readonly IEvDbMessagesSinkPublishProvider _provider;
        private readonly EvDbSinkTarget _target;

        public SpecializedTarget(IEvDbMessagesSinkPublishProvider provider, EvDbSinkTarget target)
        {
            _provider = provider;
            _target = target;
        }

        async Task IEvDbTargetedMessagesSinkPublish.PublishMessageToSinkAsync(EvDbMessage message,
                                                                        CancellationToken cancellationToken)
        {
            await _provider.PublishMessageToSinkAsync(_target, message, cancellationToken);
        }
    }


}
