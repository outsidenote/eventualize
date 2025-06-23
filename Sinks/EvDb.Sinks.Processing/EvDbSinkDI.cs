using EvDb.Core;
using EvDb.Core.Internals;
using EvDb.Sinks;
using EvDb.Sinks.Internals;
using EvDb.Sinks.Processing;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class EvDbSinkDI
{
    #region BuildHostedService

    /// <summary>
    /// Add message sink (usually sink from outbox to a stream or a queue)
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEvDbSinkRegistration BuildHostedService(
                                        this IEvDbSinkRegistrationBuilder builder)
    {
        var bag = (SinkBag)builder;
        var services = bag.Services;


        services.AddKeyedSingleton(bag.Id, bag);
        services.AddKeyedSingleton(bag.Id, (sp, key) =>
        {
            var publishers = sp.GetRequiredKeyedService<IEnumerable<IEvDbTargetedMessagesSinkPublish>>(bag.Id);
            IEvDbChangeStream changeStream = sp.GetKeyedService<IEvDbChangeStream>(bag.Id) ??
                                             sp.GetRequiredService<IEvDbChangeStream>();
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbMessagesSinkProcessor>();
            IEvDbMessagesSinkProcessor result = new EvDbMessagesSinkProcessor(logger, changeStream, publishers, bag);
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

    #endregion //  BuildHostedService

    #region BuildProcessor

    /// <summary>
    /// Add message sink (usually sink from outbox to a stream or a queue)
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IEvDbSinkRegistration BuildProcessor(
                                        this IEvDbSinkRegistrationBuilder builder)
    {
        var bag = (SinkBag)builder;
        var services = bag.Services;


        services.AddKeyedSingleton(bag.Id, bag);
        services.AddSingleton((sp) =>
        {
            var publishers = sp.GetRequiredKeyedService<IEnumerable<IEvDbTargetedMessagesSinkPublish>>(bag.Id);
            IEvDbChangeStream changeStream = sp.GetKeyedService<IEvDbChangeStream>(bag.Id) ??
                                             sp.GetRequiredService<IEvDbChangeStream>();
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = logFactory.CreateLogger<EvDbMessagesSinkProcessor>();
            IEvDbMessagesSinkProcessor result = new EvDbMessagesSinkProcessor(logger, changeStream, publishers, bag);
            return result;
        });

        var result = new SinkRegistration(services, bag.Id);
        return result;
    }

    #endregion //  BuildProcessor

    #region AddSink

    public static SinkChoices AddSink(this EvDbRegistrationEntry self)
    {
        IEvDbServiceCollectionWrapper entry = self;
        return new SinkChoices(entry.Services);
    }

    #endregion //  AddSink

    #region ForMessages

    public static IEvDbSinkRegistrationBuilder ForMessages(
                                        this SinkChoices self,
                                        EvDbStorageContextData? context = null)
    {
        IEvDbServiceCollectionWrapper entry = self;
        IServiceCollection services = entry.Services;
        var bag = new SinkBag(services);
        return bag;
    }

    #endregion //  ForMessages

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

    public record SinkRegistration : IEvDbSinkRegistration
    {
        private readonly IServiceCollection _services;
        private readonly string _id;

        public SinkRegistration(IServiceCollection services, string Id)
        {
            _services = services;
            _id = Id;
        }

        IServiceCollection IEvDbServiceCollectionWrapper.Services => _services;
        string IEvDbSinkRegistration.Id => _id;

    }

    #endregion //  SinkRegistration

    #region SinkChoices

    public readonly record struct SinkChoices : IEvDbServiceCollectionWrapper
    {
        private readonly IServiceCollection _services;

        public SinkChoices(IServiceCollection services)
        {
            _services = services;
        }

        IServiceCollection IEvDbServiceCollectionWrapper.Services => _services;
    }

    #endregion //  SinkChoices
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

