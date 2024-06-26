using EvDb.Core;
using Microsoft.AspNetCore.Mvc;
using EvDb.MinimalStructure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using System.Threading.Channels;

namespace EvDb.StressTestsWebApi.Controllers;

public class StressJob : BackgroundService
{
    private readonly ILogger<StressJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<StressOptions> _channel;
    private const int REPORT_INTERVAL = 200;

    public StressJob(
        ILogger<StressJob> logger,
        IServiceScopeFactory scopeFactory,
        Channel<StressOptions> channel)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _channel = channel;
    }

    #region ExecuteAsync

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var options = await _channel.Reader.ReadAsync(stoppingToken);
                await CreateEnvironmentAsync(options, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    #endregion //  ExecuteAsync

    #region CreateEnvironmentAsync

    public async Task CreateEnvironmentAsync(
        StressOptions options,
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start: {options}", options);
        using (var scope = _scopeFactory.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IEvDbDemoStreamFactory>();
            var storageMigration = scope.ServiceProvider.GetRequiredService<IEvDbStorageMigration>();
            try
            {
                try
                {
                    await storageMigration.CreateEnvironmentAsync(stoppingToken);
                }
                catch { }

                await RunAsync(options, factory, stoppingToken);
            }
            finally
            {
                await storageMigration.DestroyEnvironmentAsync(stoppingToken);
            }
        }
    }

    #endregion //  CreateEnvironmentAsync

    #region RunAsync

    public async Task RunAsync(
        StressOptions options,
        IEvDbDemoStreamFactory factory,
        CancellationToken stoppingToken)
    {
        (int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize) = options;

        int counter = 0;
        int occCounter = 0;
        var tasks = Enumerable.Range(0, streamsCount)
            .Select(async stream_i =>
            {
                var streamId = $"stream-{stream_i}";
                var sw = Stopwatch.StartNew();

                var ab = new ActionBlock<int>(async j =>
                {
                    Interlocked.Increment(ref counter);
                    bool success = false;
                    IEnumerable<Event1> events = CreateEvents(streamId, batchSize, j * batchSize);
                    do
                    {
                        IEvDbDemoStream stream = await factory.GetAsync(streamId, stoppingToken);
                        var tasks = events.Select(async e => await stream.AddAsync(e));
                        IEvDbEventMeta[] es = await Task.WhenAll(tasks);
                        for (int q = 0; q < es.Length; q++)
                        {
                            var e = es[q];
                        }

                        try
                        {
                            var offset0 = stream.StoredOffset;
                            int affected = await stream.StoreAsync(stoppingToken);
                            var offset1 = stream.StoredOffset;
                            success = true;
                        }
                        catch (OCCException)
                        {
                            Interlocked.Increment(ref occCounter);
                        }
                    } while (!success);
                    if (counter % REPORT_INTERVAL == 0)
                    {
                        sw.Stop();
                        _logger.LogInformation($"Count: {counter}: Avg Duration (sec): {sw.ElapsedMilliseconds / REPORT_INTERVAL / 1000.0:N3}");
                        sw = Stopwatch.StartNew();
                    }
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

        _logger.LogInformation("=================== COMPLETE ==================");

        int expectedEventsCount = writeCycleCount * batchSize;
        _logger.LogInformation($"count: {counter}");
        _logger.LogInformation($"OCC count: {occCounter}");
        for (int i = 0; i < streamsCount; i++)
        {
            var streamId = $"stream-{i}";
            var stream = await factory.GetAsync(streamId);

            if (expectedEventsCount - 1 != stream.StoredOffset)
                throw new Exception("Invalid offset");
        }
    }

    #endregion //  RunAsync

    #region CreateEvents

    static IEnumerable<Event1> CreateEvents(string streamId, int batchSize, int baseId)
    {
        return Enumerable.Range(0, batchSize)
                         .Select(k =>
                         {
                             int id = baseId + k;
                             var e = new Event1(id, $"Person {id}", baseId / batchSize);
                             return e;
                         });
    }

    #endregion //  CreateEvents
}
