using EvDb.Core;
using EvDb.Core.Internals;
using EvDb.Sinks;
using EvDb.Sinks.Processing;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbSinkDI
{
    #region Build

    /// <summary>
    /// Add message sink (usually sink from outbox to a stream or a queue)
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="target">sink target (queue name or topic name)</param>
    /// <returns></returns>
    public static IEvDbSinkRegistration Build(
                                        this IEvDbSinkRegistrationBuilder builder,
                                        EvDbSinkTarget target)
    {
        var bag = (SinkBag)builder;
        var services = bag.Services;


        services.AddKeyedSingleton(bag.Id, bag);
        services.AddKeyedSingleton(bag.Id, (sp, key) =>
        {
            var publisher = sp.GetRequiredKeyedService<IEvDbTargetedMessagesSinkPublish>(bag.Id);
            IEvDbChangeStream changeStream = sp.GetKeyedService<IEvDbChangeStream>(bag.Id) ??
                                             sp.GetRequiredService<IEvDbChangeStream>();
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbMessagesSinkProcessor>();
            var result = new EvDbMessagesSinkProcessor(logger, publisher, changeStream, bag);
            return result;
        });

        // register the processor
        services.AddHostedService(sp =>
        {
            var provider = sp.GetRequiredKeyedService<IEvDbMessagesSinkProcessor>(bag.Id);
            var result = new EvDbSinkProcessorHost(provider);
            return result;
        });

        var result = new SinkRegistration(services, bag.Id);
        return result;
    }

    #endregion //  Build

    #region AddHostedMessageSink

    public static IEvDbSinkRegistrationBuilder AddHostedMessageSink(
                                        this IEvDbRegistrationEntry entry,
                                        EvDbStorageContextData? context = null)
    {
        IServiceCollection services = entry.Services;
        var bag = new SinkBag(services); 
        return bag;
    }

    #endregion //  AddHostedMessageSink

    #region AddShard

    public static IEvDbSinkRegistrationBuilder AddShard(
                                        this IEvDbSinkRegistrationBuilder builder,
                                        EvDbShardName shard)
    {
        var bag = (SinkBag)builder;
        return bag with { Shard = shard };
    }

    #endregion //  AddShard

    #region AddFilter

    public static IEvDbSinkRegistrationBuilder AddFilter(
                                        this IEvDbSinkRegistrationBuilder builder,
                                        EvDbMessageFilter filter)
    {
        var bag = (SinkBag)builder;
        return bag with { Filter = filter };
    }

    #endregion //  AddFilter

    #region AddOptions

    public static IEvDbSinkRegistrationBuilder AddOptions(
                                        this IEvDbSinkRegistrationBuilder builder,
                                        EvDbContinuousFetchOptions options)
    {
        var bag = (SinkBag)builder;
        return bag with { Options = options };
    }

    #endregion //  AddOptions

    #region SinkRegistration

    internal record SinkRegistration(IServiceCollection Services, string Id) : IEvDbSinkRegistration
    {
    }

    #endregion //  SinkRegistration
}

#region SinkBag

internal record SinkBag(IServiceCollection Services) : IEvDbSinkRegistrationBuilder
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public EvDbShardName Shard = EvDbShardName.Default;
    public EvDbMessageFilter Filter { get; init; } = DateTimeOffset.UtcNow;
    public EvDbContinuousFetchOptions? Options { get; init; }
}

#endregion //  SinkBag

