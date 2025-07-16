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

        services.TryAddSingleton<IEvDbSinkSNSMeters, EvDbSinkSNSMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSNS>();
            var client = sp.GetRequiredService<AmazonSimpleNotificationServiceClient>();

            return new EvDbSinkProviderSNS(logger, client, EvDbSinkSNSMeters.Default);
        });

        services.AddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(topicName);
            return result;
        });

        return registration;
    }

#if APPROVED

    /// <summary>
    /// Add SNS sink for a specific queue name (if not exists) 
    /// The queue name is used as a key to retrieve the sink.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedSNSPublishProvider(this IServiceCollection services, string topicName)
    {
        return services.AddKeyedSNSPublishProvider(topicName, topicName);
    }

    /// <summary>
    /// Add SNS sink for a specific queue name (if not exists) 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">The registration key</param>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedSNSPublishProvider<TKey>(this IServiceCollection services, TKey key, string topicName)
    {
        services.TryAddSingleton<IEvDbSinkSNSMeters, EvDbSinkSNSMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSNS>();
            var client = sp.GetRequiredService<AmazonSimpleNotificationServiceClient>();

            return new EvDbSinkProviderSNS(logger, client, EvDbSinkSNSMeters.Default);
        });

        services.AddKeyedSingleton(key, (sp, _) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(topicName);
            return result;
        });

        return services;
    }

#endif // APPROVED
}
