using Amazon.SQS;
using Amazon.SQS.Model;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.Core.Internals.OtelConstants;
using static EvDb.Sinks.EvDbSinkTelemetry;

#pragma warning disable S101 // Types should be named in PascalCase

namespace EvDb.Sinks.EvDbSinkSQS;

internal class EvDbSinkProviderSQS : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderSQS> _logger;
    private readonly AmazonSQSClient _client;
    private readonly IEvDbSinkSQSMeters _meters;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public EvDbSinkProviderSQS(ILogger<EvDbSinkProviderSQS> logger,
                               AmazonSQSClient client,
                               IEvDbSinkSQSMeters meters)
    {
        _logger = logger;
        _client = client;
        _meters = meters;
    }

    private async Task PublishMessageToSinkAsync(EvDbSinkTarget target,
                                                                          EvDbMessageRecord message,
                                                                          JsonSerializerOptions? serializerOptions,
                                                                          CancellationToken cancellationToken)
    {
        ActivityContext parentContext = message.TraceParent.ToTelemetryContext();
        using var activity = OtelSinkTrace.CreateBuilder("EvDb.PublishToSQS")
                                      .WithParent(parentContext)
                                      .WithKind(ActivityKind.Producer)
                                      .AddTags(message.ToTelemetryTags())
                                      .AddTag(TAG_SINK_TARGET_NAME, target)
                                      .AddTag(TAG_STORAGE_TYPE_NAME, "SQS")
                                      .Start();

        _meters.IncrementPublish(target);

        string json = JsonSerializer.Serialize(message, serializerOptions);

        #region MessageAttributeValue messageAttributes = OTEL Context

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

        #endregion // MessageAttributeValue messageAttributes = OTEL Context

        #region var request = new SendMessageRequest(..)

        var request = new SendMessageRequest
        {
            QueueUrl = target,
            MessageBody = json,
            MessageAttributes = messageAttributes,
        };

        if (target.Value.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase))
        {
            // For FIFO queues, we need to set MessageGroupId and MessageDeduplicationId
            request.MessageGroupId = message.GetAddress().ToString();
            request.MessageDeduplicationId = message.Id.ToString("N");
        }

        #endregion //  var request = new SendMessageRequest(..)

        SendMessageResponse response = await _client.SendMessageAsync(request, cancellationToken);
        _logger.LogPublished(target, message.Id, response.MessageId, response.HttpStatusCode.ToString());
    }

    IEvDbTargetedMessagesSinkPublish IEvDbMessagesSinkPublishProvider.Create(EvDbSinkTarget target) => new SpecializedTarget(this, target);

    private sealed class SpecializedTarget : IEvDbTargetedMessagesSinkPublish
    {
        private readonly EvDbSinkProviderSQS _provider;
        private readonly EvDbSinkTarget _target;

        public SpecializedTarget(EvDbSinkProviderSQS provider, EvDbSinkTarget target)
        {
            _provider = provider;
            _target = target;
        }

        string IEvDbTargetedMessagesSinkPublish.Kind { get; } = "SQS";

        async Task IEvDbTargetedMessagesSinkPublish.PublishMessageToSinkAsync(EvDbMessage message,
                                                                        JsonSerializerOptions? serializerOptions,
                                                                        CancellationToken cancellationToken)
        {
            await _provider.PublishMessageToSinkAsync(_target, message, serializerOptions, cancellationToken);
        }
    }


}
