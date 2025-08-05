using Amazon.SQS;
using EvDb.Sinks;
using EvDb.Sinks.EvDbSinkSQS;
using EvDb.Sinks.Internals;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#pragma warning disable S101 // Types should be named in PascalCase

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// SQS extension
/// </summary>
public static class EvDbSinkSQSDI
{
    private const string PROVIDER_KEY = "SQS";

    /// <summary>
    /// Add SQS sink for a specific queue name (if not exists)
    /// </summary>
    /// <param name="registration"></param>
    /// <param name="client"
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static IEvDbSinkRegistration SendToSQS(
                            this IEvDbSinkRegistration registration,
                            AmazonSQSClient client,
                            string queueName)
    {
        var services = registration.Services;

        services.TryAddSingleton<IEvDbSinkSQSMeters, EvDbSinkSQSMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSQS>();

            return new EvDbSinkProviderSQS(logger, client, EvDbSinkSQSMeters.Default);
        });

        services.AddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(queueName);
            return result;
        });

        return registration;
    }

    /// <summary>
    /// Add SQS sink for a specific queue name (if not exists)
    /// </summary>
    /// <param name="registration"></param>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static IEvDbSinkRegistration SendToSQS(this IEvDbSinkRegistration registration, string queueName)
    {
        var services = registration.Services;

        services.TryAddSingleton<IEvDbSinkSQSMeters, EvDbSinkSQSMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSQS>();
            var client = sp.GetRequiredService<AmazonSQSClient>();

            return new EvDbSinkProviderSQS(logger, client, EvDbSinkSQSMeters.Default);
        });

        services.AddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(queueName);
            return result;
        });

        return registration;
    }

#if APPROVED

    /// <summary>
    /// Add SQS sink for a specific queue name (if not exists) 
    /// The queue name is used as a key to retrieve the sink.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedSQSPublishProvider(this IServiceCollection services, string queueName)
    {
        return services.AddKeyedSQSPublishProvider(queueName, queueName);
    }

    /// <summary>
    /// Add SQS sink for a specific queue name (if not exists) 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">The registration key</param>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedSQSPublishProvider<TKey>(this IServiceCollection services, TKey key, string queueName)
    {
        services.TryAddSingleton<IEvDbSinkSQSMeters, EvDbSinkSQSMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSQS>();
            var client = sp.GetRequiredService<AmazonSQSClient>();

            return new EvDbSinkProviderSQS(logger, client, EvDbSinkSQSMeters.Default);
        });

        services.AddKeyedSingleton(key, (sp, _) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(queueName);
            return result;
        });

        return services;
    }

#endif // APPROVED
}
