namespace EvDb.Core.Tests;

using Cocona;
using EvDb.MinimalStructure;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;
using EvDb.Core.Store;
using EvDb.UnitTests;

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
                                c => c.UseSqlServerStoreForEvDbStream(),
                                EvDbStorageContext.CreateWithEnvironment())
                        .AddTopics(tg => tg.CreateTopicGroup("TestGroup",EvDbSchoolStreamTopicOptions.Topic1, EvDbSchoolStreamTopicOptions.Topic2))
                        .AddTopics(tg => tg.CreateTopicGroup("TestGroup2", EvDbSchoolStreamTopicOptions.Topic1, EvDbSchoolStreamTopicOptions.Topic3))
                            //.Topics(c =>
                            //{
                            //    c.CreateTopicGroup(x => [x.Topic1, x.Topic2])
                            //            .WithTransformation<T>()
                            //            .WithTransformation(x => JsonSerializer.Serialize(x.Payload));
                            //})
                            .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection"))
                            .ForALL(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection-server1"))
                            .ForStudentStats(c => c.UseSqlServerForEvDbSnapshot("EvDbSqlServerConnection2"));
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
            await stream.AddAsync(new Event1(1, $"Person {k}", k));

        }
    }
}