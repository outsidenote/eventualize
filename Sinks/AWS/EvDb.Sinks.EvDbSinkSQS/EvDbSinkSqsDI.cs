using Amazon.SQS;
using EvDb.Sinks;
using EvDb.Sinks.EvDbSinkSQS;
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
    /// <param name="queueName"></param>
    /// <returns></returns>
    public static IEvDbSinkRegistration TryAddSinkSQS(this IEvDbSinkRegistration registration, string queueName)
    {
        var services = registration.Services;
        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY, (sp, _) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSQS>();
            var client = sp.GetRequiredService<AmazonSQSClient>();

            return new EvDbSinkProviderSQS(logger, client, queueName);
        });

        services.TryAddKeyedSingleton(registration.Id, (sp, key) =>
        {
            var sink = sp.GetRequiredKeyedService<IEvDbMessagesSinkPublishProvider>(PROVIDER_KEY);
            IEvDbTargetedMessagesSinkPublish result = sink.Create(queueName);
            return result;
        });


        services.TryAddKeyedSingleton<IEvDbMessagesSinkPublishProvider>(queueName, (sp, key) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbSinkProviderSQS>();
            var client = sp.GetRequiredService<AmazonSQSClient>();
            return new EvDbSinkProviderSQS(logger, client, (string)key!);
        });

        return registration;
    }
}
