using Confluent.Kafka;
using EvDb.Sinks;
using EvDb.Sinks.EvDbSinkKafka;
using EvDb.Sinks.Internals;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
#pragma warning disable S101 // Types should be named in PascalCase

namespace Microsoft.Extensions.DependencyInjection;


public static class EvDbSinkKafkaDI
{
    private const string PROVIDER_KEY = "Kafka";

    public static IEvDbSinkRegistration SendToKafka(
                                                this IEvDbSinkRegistration registration,
                                                IProducer<string, string> client,
                                                EvDbSinkTarget topicName)
    {
        var services = registration.Services;

        services.TryAddSingleton<IEvDbSinkKafkaMeters, EvDbSinkKafkaMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderKafka>();

            return new EvDbSinkProviderKafka(logger, client, EvDbSinkKafkaMeters.Default);
        });


        services.AddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(topicName);
            return result;
        });

        return registration;
    }

    public static IEvDbSinkRegistration SendToKafka(this IEvDbSinkRegistration registration, EvDbSinkTarget topicName)
    {
        var services = registration.Services;

        services.TryAddSingleton<IEvDbSinkKafkaMeters, EvDbSinkKafkaMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderKafka>();
            var client = sp.GetRequiredService<IProducer<string, string>>();

            return new EvDbSinkProviderKafka(logger, client, EvDbSinkKafkaMeters.Default);
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
    /// Add Kafka sink for a specific queue name (if not exists) 
    /// The queue name is used as a key to retrieve the sink.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedKafkaPublishProvider(this IServiceCollection services, string topicName)
    {
        return services.AddKeyedKafkaPublishProvider(topicName, topicName);
    }

    /// <summary>
    /// Add Kafka sink for a specific queue name (if not exists) 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key">The registration key</param>
    /// <param name="topicName"></param>
    /// <returns></returns>
    public static IServiceCollection AddKeyedKafkaPublishProvider<TKey>(this IServiceCollection services, TKey key, string topicName)
    {
        services.TryAddSingleton<IEvDbSinkKafkaMeters, EvDbSinkKafkaMeters>();

        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderKafka>();
            var client = sp.GetRequiredService<IProducer<string, string>>();

            return new EvDbSinkProviderKafka(logger, client, EvDbSinkKafkaMeters.Default);
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
