using Amazon.SimpleNotificationService;
using EvDb.Sinks;
using EvDb.Sinks.EvDbSinkSNS;
using EvDb.Sinks.Internals;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#pragma warning disable S101 // Types should be named in PascalCase

namespace Microsoft.Extensions.DependencyInjection;


public static class EvDbSinkSNSDI
{
    private const string PROVIDER_KEY = "SNS";

    public static IEvDbSinkRegistration SendToSNS(this IEvDbSinkRegistration registration, EvDbSinkTarget topicName)
    {
        var services = registration.Services;
        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSNS>();
            var client = sp.GetRequiredService<AmazonSimpleNotificationServiceClient>();

            return new EvDbSinkProviderSNS(logger, client);
        });

        services.TryAddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(topicName);
            return result;
        });

        return registration;
    }

}
