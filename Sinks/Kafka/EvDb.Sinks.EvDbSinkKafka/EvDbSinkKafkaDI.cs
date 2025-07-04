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

    public static IEvDbSinkRegistration SendToKafka(this IEvDbSinkRegistration registration, EvDbSinkTarget topicName)
    {
        var services = registration.Services;

        services.AddSingleton<IEvDbSinkKafkaMeters, EvDbSinkKafkaMeters>();

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

}
