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

    #region Ctor

    public StressTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
        CoconaAppBuilder builder = CoconaApp.CreateBuilder();
        var services = builder.Services;
        // services.AddSingleton(StorageContext);
        services.AddEvDb()
            .AddDemoStreamFactory(c => c.UseSqlServerStoreForEvDbStream(), StorageContext)
            .DefaultSnapshotConfiguration(c => c.UseSqlServerForEvDbSnapshot());
        Otel(builder);
        var sp = services.BuildServiceProvider();
        _factory = sp.GetRequiredService<IEvDbDemoStreamFactory>();
    }

    #endregion //  Ctor

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
                                //.AddOtlpExporter()
                                //.AddOtlpExporter("alloy", o => o.Endpoint = new Uri("http://localhost:12345"))
                                .AddOtlpExporter("aspire", o => o.Endpoint = new Uri("http://localhost:18889")));
    }

    #endregion // Otel

    [Theory]
    [Trait("Category", "Stress")]
    //[InlineData(10, 1, 1, 2)]
    //[InlineData(10, 1, 2, 2)]
    //[InlineData(100, 10, 10, 5)]
    //[InlineData(100, 1, 10, 5)]
    //[InlineData(300, 1, 10, 5)]
    //[InlineData(100, 3, 10, 5)]
    [InlineData(500, 3, 10, 5)]
    public async Task StreamFactory_Stress_Succeed(
        int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize)
    {
        int counter = 0;
        int occCounter = 0;
        var tasks = Enumerable.Range(0, streamsCount)
            .Select(async i =>
            {
                var streamId = $"stream-{i}";

                var ab = new ActionBlock<int>(async j =>
                {
                    Interlocked.Increment(ref counter);
                    bool success = false;
                    IEnumerable<Event1> events = CreateEvents(streamId, batchSize, j * batchSize);
                    do
                    {
                        IEvDbDemoStream stream = await _factory.GetAsync(streamId);
                        var tasks = events.Select(async e => await stream.AddAsync(e));
                        IEvDbEventMeta[] es = await Task.WhenAll(tasks);
                        for (int q = 0; q < es.Length; q++)
                        {
                            var e = es[q];
                            //Assert.Equal(q, e.StreamCursor.Offset % batchSize);
                        }
                        Assert.Equal(batchSize, stream?.CountOfPendingEvents);
                        try
                        {
                            var offset0 = stream!.StoredOffset;
                            int affected = await stream!.StoreAsync();
                            Assert.Equal(batchSize, affected);
                            var offset1 = stream.StoredOffset;
                            //Assert.Equal(batchSize, offset1 - offset0);
                            //Assert.Equal(0 ,(offset1 + 1) % batchSize);

                            success = true;
                        }
                        catch (OCCException)
                        {
                            Interlocked.Increment(ref occCounter);
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
        _output.WriteLine($"count: {counter}");
        _output.WriteLine($"OCC count: {occCounter}");
        for (int i = 0; i < streamsCount; i++)
        {
            var streamId = $"stream-{i}";
            var stream = await _factory.GetAsync(streamId);

            Assert.Equal(expectedEventsCount - 1, stream.StoredOffset);
            //Assert.Equal(expectedEventsCount, stream.Views.Count);
        }
    }

    [Theory]
    [Trait("Category", "Stress")]
    //[InlineData(10, 1, 1, 2)]
    //[InlineData(10, 1, 2, 2)]
    [InlineData(50, 1, 10, 2)]
    //[InlineData(100, 10, 10, 5)]
    public async Task StreamFactory_Stress_Bad_Practice_Succeed(
        int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize)

    {
        int counter = 0;
        int occCounter = 0;
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
                        var tasks = Enumerable.Range(0, batchSize)
                                              .Select(async k =>
                                        {
                                            var e = new Event1(1, $"Person [{i}]: {j} in <{k}>", i * j * k);
                                            await stream.AddAsync(e);
                                        });
                        await Task.WhenAll(tasks);
                        try
                        {
                            await stream.StoreAsync();
                            success = true;
                        }
                        catch (OCCException)
                        {
                            Interlocked.Increment(ref occCounter);
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
        _output.WriteLine($"OCC count: {occCounter}");
        for (int i = 0; i < streamsCount; i++)
        {
            var streamId = $"stream-{i}";
            var stream = await _factory.GetAsync(streamId);

            Assert.Equal(expectedEventsCount, stream.Views.Count);
            Assert.Equal(expectedEventsCount - 1, stream.StoredOffset);
        }
    }

    private static IEnumerable<Event1> CreateEvents(string streamId, int batchSize, int baseId)
    {
        return Enumerable.Range(0, batchSize)
                         .Select(k =>
                            {
                                int id = baseId + k;
                                var e = new Event1(id, $"Person {id}", baseId / batchSize);
                                return e;
                            });
    }
}