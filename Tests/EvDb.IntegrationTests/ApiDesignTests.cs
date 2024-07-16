namespace EvDb.Core.Tests;

using Cocona;
using EvDb.MinimalStructure;
using FakeItEasy;
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
        /*      EvDbStorageContext.CreateWithEnvironment()
                                  .AddStream
                                .AddSqlServerStreamStore()*/
        //services.AddEvDbStore()
        //        .AddSqlServerStreamStore();
        //services.AddEvDbDemoStreamFactory();
        services.AddEvDbDemoStreamFactory(
                        EvDbStorageContext.CreateWithEnvironment(), // + friendly overloads
                        // keyed injection under the stream address
                        connectionStringOrCongigKey: "sql-server",
                        // mode: EvDbModes.ReadOnly, // optional TBD
                        viewStorageBuilder => // flow the context
                        {
                            // keyed injection under the view address
                            viewStorageBuilder
                                .UseDefault() // expose IEvDbViewStorageBuilder
                                    .WithSqlServer(connectionStringOrCongigKey: "sql-server")
                                .ForInterval // expose IEvDbViewStorageBuilder
                                    .UseMongoDb(connectionStringOrCongigKey: "mongo") // provider layer extenssion
                                .ForCount
                                    .UsePostgres(connectionStringOrCongigKey: "postres", 
                                                  options => options.Interval = TimeSpan.FromSeconds(5))
                                //.ForViewZ // TBD
                                //    .UseKafka(connectionStringOrCongigKey: "kafka-evdb-sync", 
                                //                  options => options.Throttle = TimeSpan.FromSeconds(5))
                                .ForViewX 
                                    //.WithTiggerDb(connectionStringOrCongigKey: "tigger");
                        });
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbDemoStreamFactory>();
    }

    [Fact]
    public async Task ApiDesignPlaygroundTest()
    {
        string streamId = GenerateStreamId();
        IEvDbDemoStream stream = _factory.Create(streamId);
        for (int k = 0; k < 4; k++)
        {
            await stream.AddAsync(new Event1(1, $"Person {k}", k));

        }
        //stream.
    }
}