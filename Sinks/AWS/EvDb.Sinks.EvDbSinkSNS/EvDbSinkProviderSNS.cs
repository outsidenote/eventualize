using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.Core.Internals.OtelConstants;
using static EvDb.Sinks.EvDbSinkTelemetry;

#pragma warning disable S101 // Types should be named in PascalCase

namespace EvDb.Sinks.EvDbSinkSNS;

internal class EvDbSinkProviderSNS : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderSNS> _logger;
    private readonly AmazonSimpleNotificationServiceClient _client;
    private readonly IEvDbSinkSNSMeters _meters;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public EvDbSinkProviderSNS(ILogger<EvDbSinkProviderSNS> logger,
                               AmazonSimpleNotificationServiceClient client,
                               IEvDbSinkSNSMeters meters)
    {
        _logger = logger;
        _client = client;
        _meters = meters;
    }

    async Task IEvDbMessagesSinkPublishProvider.PublishMessageToSinkAsync(EvDbSinkTarget target,
                                                                          EvDbMessageRecord message,
                                                                          JsonSerializerOptions? serializerOptions,
                                                                          CancellationToken cancellationToken)
    {
        ActivityContext parentContext = message.TelemetryContext.ToTelemetryContext();
        using var activity = OtelSinkTrace.CreateBuilder("EvDb.PublishToSNS")
                                      .WithParent(parentContext)
                                      .WithKind(ActivityKind.Producer)
                                      .AddTags(message.ToTelemetryTags())
                                      .AddTag(TAG_SINK_TARGET_NAME, target)
                                      .AddTag(TAG_STORAGE_TYPE_NAME, "SNS")
                                      .Start();

        _meters.IncrementPublish(target);

        string json = JsonSerializer.Serialize(message, serializerOptions);
        string topicArn = await _client.GetOrCreateTopicAsync(target, _logger, cancellationToken);

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

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = json,
            MessageAttributes = messageAttributes,
            // MessageStructure = "json"
        };
        if (target.Value.EndsWith(".fifo", StringComparison.OrdinalIgnoreCase))
        {
            request.MessageGroupId = message.GetAddress().ToString();
            request.MessageDeduplicationId = message.Id.ToString("N");
        }

        PublishResponse response = await _client.PublishAsync(request, cancellationToken);
        _logger.LogPublished(target, message.Id, response.MessageId, response.HttpStatusCode.ToString());
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

        string IEvDbTargetedMessagesSinkPublish.Kind { get; } = "SNS";

        async Task IEvDbTargetedMessagesSinkPublish.PublishMessageToSinkAsync(EvDbMessage message,
                                                                        CancellationToken cancellationToken)
        {
            await _provider.PublishMessageToSinkAsync(_target, message, cancellationToken);
        }
    }
}
