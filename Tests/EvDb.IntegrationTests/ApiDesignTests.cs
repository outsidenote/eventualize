namespace EvDb.Core.Tests;

using Cocona;
using EvDb.MinimalStructure;
using EvDb.UnitTests;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;

public class ApiDesignTests
{
    private readonly IEvDbDemoStreamFactory _factory;
    private readonly ITestOutputHelper _output;
    private static string GenerateStreamId() => $"test-stream-{Guid.NewGuid():N}";

    public ApiDesignTests(ITestOutputHelper output)
    {
        _output = output;
        var builder = CoconaApp.CreateBuilder();
        var services = builder.Services;

        services.AddEvDb() // return IEvDbBuilder that will be used as the hook for the generated extensions method
                           // return IEvDbSchoolBuilder that will be used as the hook for the generated extensions method
                        .AddSchoolStreamFactory(
                                c => c.UseSqlServerStoreForEvDbStream(Enumerable.Empty<IEvDbOutboxTransformer>()),
                                EvDbStorageContext.CreateWithEnvironment("master"))
                        .AddOutbox(tg => tg.CreateShard("TestGroup", EvDbSchoolOutbox.Channels.Channel1, EvDbSchoolOutbox.Channels.Channel2))
                        .AddOutbox(tg => tg.CreateShard("TestGroup2", EvDbSchoolOutbox.Channels.Channel1, EvDbSchoolOutbox.Channels.Channel3))
                            //.Topics(c =>
                            //{
                            //    c.CreateTopicGroup(x => [x.Topic1, x.Topic2])
                            //            .WithTransformation<TState>()
                            //            .WithTransformation(x => JsonSerializer.Serialize(x.Payload));
                            //})
                            .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection"))
                            .ForALL(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection-server1"))
                            .ForStudentStats(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection2"));
        services.AddEvDb()
                .AddChangeStream(StoreType.MongoDB);

        services.AddEvDb()
                .AddSink()
                .ForMessages()
                .AddShard(EvDbShardName.Default)
                .AddFilter(EvDbMessageFilter.Create(DateTimeOffset.UtcNow))
                .AddOptions(EvDbContinuousFetchOptions.ContinueWhenEmpty)
                .BuildHostedService()
                .SendToSQS("app-commands")
                .SendToSNS("app-internal-messages")
                .SendToSNS("app-vip-messages");
                
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbDemoStreamFactory>();
    }

    [Fact(Skip = "API Design")]
    public async Task ApiDesignPlaygroundTest()
    {
        string streamId = GenerateStreamId();
        IEvDbDemoStream stream = _factory.Create(streamId);
        for (int k = 0; k < 4; k++)
        {
            await stream.AppendAsync(new Event1(1, $"Person {k}", k));

        }
    }
}