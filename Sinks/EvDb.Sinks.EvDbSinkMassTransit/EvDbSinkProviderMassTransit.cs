using EvDb.Core;
using EvDb.Core.Adapters;
using MassTransit;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text.Json;
using static EvDb.Core.Internals.OtelConstants;
using static EvDb.Sinks.EvDbSinkTelemetry;

namespace EvDb.Sinks.EvDbSinkMassTransit;

internal class EvDbSinkProviderMassTransit : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderMassTransit> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEvDbSinkMassTransitMeters _meters;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public EvDbSinkProviderMassTransit(ILogger<EvDbSinkProviderMassTransit> logger,
                                       IPublishEndpoint publishEndpoint,
                                       IEvDbSinkMassTransitMeters meters)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _meters = meters;
    }

    public async Task PublishMessageToSinkAsync(EvDbSinkTarget target,
                                                EvDbMessageRecord message,
                                                JsonSerializerOptions? serializerOptions,
                                                CancellationToken cancellationToken)
    {
        var parentContext = message.TelemetryContext.ToTelemetryContext();
        using var activity = OtelSinkTrace.CreateBuilder("EvDb.PublishToMassTransit")
                                          .WithParent(parentContext)
                                          .WithKind(ActivityKind.Producer)
                                          .AddTags(message.ToTelemetryTags())
                                          .AddTag(TAG_SINK_TARGET_NAME, target)
                                          .AddTag(TAG_STORAGE_TYPE_NAME, "MassTransit")
                                          .Start();

        _meters.IncrementPublish(target);

        var propagationContext = new PropagationContext(activity?.Context ?? default, Baggage.Current);

        // Create MassTransit headers with trace context
        var headers = new Dictionary<string, object>();
        Propagator.Inject(propagationContext, headers, (carrier, key, value) => carrier[key] = value);

        var wrapped = new MassTransitEnvelope
        {
            Payload = JsonSerializer.Serialize(message, serializerOptions),
            Target = target.Value,
            Metadata = headers,
            MessageId = message.Id
        };

        await _publishEndpoint.Publish(wrapped, context =>
        {
            foreach (var header in wrapped.Metadata)
            {
                context.Headers.Set(header.Key, header.Value);
            }

            context.MessageId = message.Id;
            context.CorrelationId = message.Id; // or any correlation logic
        }, cancellationToken);

        _logger.LogPublished(target, message.Id, message.Id.ToString(), "Published");
    }

    public IEvDbTargetedMessagesSinkPublish Create(EvDbSinkTarget target) =>
        new SpecializedTarget(this, target);

    private sealed class SpecializedTarget : IEvDbTargetedMessagesSinkPublish
    {
        private readonly IEvDbMessagesSinkPublishProvider _provider;
        private readonly EvDbSinkTarget _target;

        public SpecializedTarget(IEvDbMessagesSinkPublishProvider provider, EvDbSinkTarget target)
        {
            _provider = provider;
            _target = target;
        }

        public string Kind => "MassTransit";

        Task IEvDbTargetedMessagesSinkPublish.PublishMessageToSinkAsync(EvDbMessage message, CancellationToken cancellationToken) =>
            _provider.PublishMessageToSinkAsync(_target, message, cancellationToken);
    }
}
