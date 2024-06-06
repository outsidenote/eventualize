using Cocona;
using Cocona.Builder;
using EvDb.MinimalStructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class StressTests : IntegrationTests
{
    private readonly IEvDbDemoStreamFactory _factory;

    public StressTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
        CoconaAppBuilder builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        services.AddSingleton(_storageAdapter);
        services.AddEvDbDemoStreamFactory();
        Otel(builder);
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbDemoStreamFactory>();
    }

    #region Otel

    private static void Otel(CoconaAppBuilder builder)
    {
        #region // Logging

        //ILoggingBuilder loggingBuilder = builder.Logging;
        //loggingBuilder.AddOpenTelemetry(logging =>
        //{
        //    var resource = ResourceBuilder.CreateDefault();
        //    logging.SetResourceBuilder(resource.AddService(
        //                    builder.Environment.ApplicationName));
        //    logging.IncludeFormattedMessage = true;
        //    logging.IncludeScopes = true;
        //    logging.AddOtlpExporter()
        //           .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
        //           .AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
        //           .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
        //});

        //loggingBuilder.Configure(x =>
        //{
        //    x.ActivityTrackingOptions = ActivityTrackingOptions.SpanId
        //      | ActivityTrackingOptions.TraceId
        //      | ActivityTrackingOptions.ParentId
        //      | ActivityTrackingOptions.Tags;
        //    // | ActivityTrackingOptions.TraceState;
        //});

        #endregion // Logging}

        var services = builder.Services;
        services.AddOpenTelemetry()
                    .ConfigureResource(resource =>
                                   resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing =>
            {
                tracing
                        .AddEvDbInstrumentation()
                        .AddSqlClientInstrumentation(o =>
                        {
                            o.SetDbStatementForText = true;
                            o.SetDbStatementForStoredProcedure = true;
                        })
                        .SetSampler<AlwaysOnSampler>()
                        //.AddOtlpExporter()
                        .AddOtlpExporter("jaeger", o => o.Endpoint = new Uri("http://localhost:4327/"))
                        //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345/"))
                        .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889"));
            })
            .WithMetrics(meterBuilder =>
                    meterBuilder.AddEvDbInstrumentation()
                                .AddProcessInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddAspNetCoreInstrumentation()
                                .AddPrometheusExporter()
                                //.AddOtlpExporter()
                                //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345"))
                                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889")));
    }

    #endregion // Otel

    [Theory]
    [Trait("Category", "Stress")]
    //[InlineData(10, 1, 1, 2)]
    [InlineData(10, 1, 2, 2)]
    //[InlineData(100, 10, 10, 5)]
    public async Task StreamFactory_Stress_Succeed(
        int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize)

    {
        int counter = 0;
        int retryCounter = 0;
        var tasks = Enumerable.Range(0, streamsCount)
            .Select(async i =>
            {
                var streamId = $"stream-{i}";

                var ab = new ActionBlock<int>(async j =>
                {
                    Interlocked.Increment(ref counter);
                    bool success = false;
                    do
                    {
                        var stream = await _factory.GetAsync(streamId);
                        Interlocked.Increment(ref retryCounter);
                        var tasks = Enumerable.Range(0, batchSize)
                                              .Select(async k =>
                                        {
                                            var e = new Event1(1, $"Person [{i}]: {j} in <{k}>", i * j * k);
                                            await stream.AddAsync(e);
                                        });
                        await Task.WhenAll(tasks);
                        try
                        {
                            await stream.SaveAsync();
                            success = true;
                        }
                        catch (OCCException)
                        {
                        }
                    } while (!success);
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = degreeOfParallelismPerStream,
                });

                for (int j = 0; j < writeCycleCount; j++)
                {
                    ab.Post(j);
                }

                ab.Complete();
                await ab.Completion;
            });
        await Task.WhenAll(tasks);

        int expectedEventsCount = writeCycleCount * batchSize;
        Assert.Equal(writeCycleCount * streamsCount, counter);
        _output.WriteLine($"Retry count: {retryCounter}");
        for (int i = 0; i < streamsCount; i++)
        {
            var streamId = $"stream-{i}";
            var stream = await _factory.GetAsync(streamId);

            Assert.Equal(expectedEventsCount, stream.Views.Count);
            Assert.Equal(expectedEventsCount - 1, stream.StoreOffset);
        }
    }

    [Theory]
    [Trait("Category", "Stress")]
    //[InlineData(10, 1, 1, 2)]
    //[InlineData(10, 1, 2, 2)]
    //[InlineData(50, 1, 10, 2)]
    [InlineData(100, 10, 10, 5)]
    public async Task StreamFactory_Stress_Bad_Practice_Succeed(
        int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize)

    {
        int counter = 0;
        int retryCounter = 0;
        var tasks = Enumerable.Range(0, streamsCount)
            .Select(async i =>
            {
                var streamId = $"stream-{i}";
                var rootStream = await _factory.GetAsync(streamId);

                var ab = new ActionBlock<int>(async j =>
                {
                    var stream = rootStream;
                    Interlocked.Increment(ref counter);
                    bool success = false;
                    do
                    {
                        Interlocked.Increment(ref retryCounter);
                        var tasks = Enumerable.Range(0, batchSize)
                                              .Select(async k =>
                                        {
                                            var e = new Event1(1, $"Person [{i}]: {j} in <{k}>", i * j * k);
                                            await stream.AddAsync(e);
                                        });
                        await Task.WhenAll(tasks);
                        try
                        {
                            await stream.SaveAsync();
                            success = true;
                        }
                        catch (OCCException)
                        {
                            //await Task.Yield();
                            rootStream = await _factory.GetAsync(streamId);
                        }
                    } while (!success);
                }, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = degreeOfParallelismPerStream,
                });

                for (int j = 0; j < writeCycleCount; j++)
                {
                    ab.Post(j);
                }

                ab.Complete();
                await ab.Completion;
            });
        await Task.WhenAll(tasks);

        int expectedEventsCount = writeCycleCount * batchSize;
        Assert.Equal(writeCycleCount * streamsCount, counter);
        _output.WriteLine($"Retry count: {retryCounter}");
        for (int i = 0; i < streamsCount; i++)
        {
            var streamId = $"stream-{i}";
            var stream = await _factory.GetAsync(streamId);

            Assert.Equal(expectedEventsCount, stream.Views.Count);
            Assert.Equal(expectedEventsCount - 1, stream.StoreOffset);
        }
    }
}