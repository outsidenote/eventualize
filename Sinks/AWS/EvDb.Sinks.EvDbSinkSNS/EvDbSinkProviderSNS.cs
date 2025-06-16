using Amazon.SimpleNotificationService;
using EvDb.Core;
using Microsoft.Extensions.Logging;
#pragma warning disable S101 // Types should be named in PascalCase

namespace EvDb.Sinks.EvDbSinkSNS;

internal class EvDbSinkProviderSNS : IEvDbMessagesSinkPublishProvider
{
    private readonly ILogger<EvDbSinkProviderSNS> _logger;
    private readonly AmazonSimpleNotificationServiceClient _client;

    public EvDbSinkProviderSNS(ILogger<EvDbSinkProviderSNS> logger,
                               AmazonSimpleNotificationServiceClient client)
    {
        _logger = logger;
        _client = client;
    }

    async Task IEvDbMessagesSinkPublishProvider.PublishMessageToSinkAsync(EvDbSinkTarget target,
                                                                          EvDbMessage message,
                                                                          CancellationToken cancellationToken)
    {
        // TODO: serialize message
        // TODO: OTEL propogation
        // TODO: translate and cache topicArn from topic

        //string payload = message.Payload;
        //var request = new PublishRequest
        //{
        //    TopicArn = topicArn,
        //    Message = change,
        //    MessageAttributes = ,
        //    MessageGroupId= message.ShardName.ToString() + (EvDbStreamAddress)message.StreamCursor,
        //    MessageDeduplicationId = message.Id.ToString("N"), // Use a unique identifier for deduplication
        //    MessageStructure = message.SerializeType == EvDbSerializeType.default ? "json", // Assuming the message is in JSON format

        //};

        //await _client.PublishAsync(request, cancellationToken);
        throw new NotImplementedException();
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
