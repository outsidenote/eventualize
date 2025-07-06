
using Confluent.Kafka;
using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.Core.Internals.OtelConstants;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace EvDb.Sinks.EvDbSinkKafka;

internal class EvDbSinkProviderKafka : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderKafka> _logger;
    private readonly IProducer<string, string> _producer;
    private readonly IEvDbSinkKafkaMeters _meters;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public EvDbSinkProviderKafka(
        ILogger<EvDbSinkProviderKafka> logger,
        IProducer<string, string> producer,
        IEvDbSinkKafkaMeters meters)
    {
        _logger = logger;
        _producer = producer;
        _meters = meters;
    }

    async Task IEvDbMessagesSinkPublishProvider.PublishMessageToSinkAsync(
        EvDbSinkTarget target,
        EvDbMessageRecord message,
        JsonSerializerOptions? serializerOptions,
        CancellationToken cancellationToken)
    {
        // Set up tracing context
        ActivityContext parentContext = message.TelemetryContext.ToTelemetryContext();
        using var activity = OtelSinkTrace.CreateBuilder("EvDb.PublishToKafka")
            .WithParent(parentContext, OtelParentRelation.Link)
            .WithKind(ActivityKind.Producer)
            .AddTags(message.ToTelemetryTags())
            .AddTag(TAG_SINK_TARGET_NAME, target)
            .AddTag(TAG_STORAGE_TYPE_NAME, "Kafka")
            .Start();

        // Increment metrics
        _meters.IncrementPublish(target);

        // Serialize payload
        string json = JsonSerializer.Serialize(message, serializerOptions);

        // Prepare Kafka message
        var kafkaMsg = new Message<string, string>
        {
            Key = message.GetAddress().ToString(),
            Value = json,
            Headers = new Headers()
        };

        // Inject OTEL trace context into Kafka headers
        var propagationContext = new PropagationContext(
                                           activity?.Context ?? default,
                                           Baggage.Create());
        Propagator.Inject(propagationContext, kafkaMsg.Headers, InjectKafkaHeader);

        static void InjectKafkaHeader(Headers headers, string key, string value)
        {
            headers.Remove(key); // avoid duplicates
            headers.Add(key, System.Text.Encoding.UTF8.GetBytes(value));
        }

        // Send the message
        DeliveryResult<string, string> response;
        try
        {
            response = await _producer.ProduceAsync(target.Value, kafkaMsg, cancellationToken);

            _logger.LogPublished(target, message.Id, response.Message.Key, response.Status.ToString(), response.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogPublishedError(target, ex);
            throw;
        }

    }

    IEvDbTargetedMessagesSinkPublish IEvDbMessagesSinkPublishProvider.Create(EvDbSinkTarget target)
        => new SpecializedTarget(this, target);

    private sealed class SpecializedTarget : IEvDbTargetedMessagesSinkPublish
    {
        private readonly IEvDbMessagesSinkPublishProvider _provider;
        private readonly EvDbSinkTarget _target;

        public SpecializedTarget(
            IEvDbMessagesSinkPublishProvider provider,
            EvDbSinkTarget target)
        {
            _provider = provider;
            _target = target;
        }

        public string Kind { get; } = "Kafka";

        public async Task PublishMessageToSinkAsync(EvDbMessage message, CancellationToken cancellationToken)
        {
            await _provider.PublishMessageToSinkAsync(_target, message, cancellationToken);
        }
    }
}
