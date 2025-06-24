using EvDb.Core;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace EvDb.StressTestsWebApi.Controllers;

public class StressJob : BackgroundService
{
    private readonly ILogger<StressJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<StressOptions> _channel;
    private const int REPORT_CYCLE = 300;

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
            var storageMigration = scope.ServiceProvider.GetRequiredService<IEvDbStorageAdmin>();
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
        string streamPrefix = options.StreamPrefix;

        int counter = 0, lastCount = 0;
        int occCounter = 0;
        var queue = new ConcurrentQueue<string>();
        using var timer = new Timer((_) =>
        {
            var perSec = counter - lastCount;
            lastCount = counter;
            _logger.LogInformation("------------- 5s ------------------");
            int x = 0;
            while (queue.TryDequeue(out string? message) && x++ < 4)
            {
                _logger.LogInformation(message);
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            _logger.LogInformation($"Event per second: {perSec / 5:N0}, Count = {lastCount:N0}");
            Console.ResetColor();
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        int totalAffectedEvents = 0;
        int totalAffectedOutbox = 0;

        var tasks = Enumerable.Range(0, streamsCount)
            .Select(async stream_i =>
            {
                int streamAffectedEvents = 0;
                int streamAffectedOutbox = 0;
                int occ = 0;

                var streamId = $"{streamPrefix}-{stream_i}";
                var sw = Stopwatch.StartNew();

                var ab = new ActionBlock<int>(async j =>
                {
                    var count = Interlocked.Increment(ref counter);
                    bool success = false;
                    IEnumerable<SomethingHappened> events = CreateEvents(streamId, batchSize, j * batchSize);
                    IEnumerable<FaultOccurred> faultEvents = CreateFaultEvents(streamId, batchSize, j * batchSize);
                    do
                    {
                        IEvDbDemoStream stream = await factory.GetAsync(streamId, stoppingToken);
                        var tasks = events.Select(async e => await stream.AppendAsync(e));
                        await Task.WhenAll(tasks);
                        tasks = faultEvents.Select(async e => await stream.AppendAsync(e));
                        await Task.WhenAll(tasks);

                        try
                        {
                            (int affectedEvents, var affectedOutbox) = await stream.StoreAsync();
                            Interlocked.Add(ref totalAffectedEvents, affectedEvents);
                            Interlocked.Add(ref totalAffectedOutbox, affectedOutbox.Values.Sum());
                            Interlocked.Add(ref streamAffectedEvents, affectedEvents);
                            Interlocked.Add(ref streamAffectedOutbox, affectedOutbox.Values.Sum());
                            success = true;
                        }
                        catch (OCCException)
                        {
                            Interlocked.Increment(ref occCounter);
                            Interlocked.Increment(ref occ);
                        }
                    } while (!success);
                    if (counter % REPORT_CYCLE == 0)
                    {
                        sw.Stop();
                        double secound = sw.Elapsed.TotalSeconds;
                        double perSeconds = REPORT_CYCLE * batchSize / secound;
                        double cyclePerSeconds = REPORT_CYCLE / secound;
                        double eventsSeconds = streamAffectedEvents / secound;
                        double outboxSeconds = streamAffectedOutbox / secound;
                        double occSeconds = occ / secound;
                        Interlocked.Exchange(ref streamAffectedEvents, 0);
                        Interlocked.Exchange(ref streamAffectedOutbox, 0);
                        Interlocked.Exchange(ref occ, 0);
                        queue.Enqueue($"""
                                ------------------------------------------
                                Total Cycles: {count}
                                    events: {eventsSeconds:N0} per second, {totalAffectedEvents} total
                                    outbox: {outboxSeconds:N0} per second, {totalAffectedOutbox} total
                                    {cyclePerSeconds:N0} cycle per second
                                    {occSeconds:N0} OCC per seconds
                                    Validation!!: {perSeconds:N0} cycle * batch per seconds
                                """);
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
            var streamId = $"{streamPrefix}-{i}";
            var stream = await factory.GetAsync(streamId);

            if (expectedEventsCount - 1 != stream.StoredOffset)
                throw new Exception("Invalid offset");
        }
    }

    #endregion //  RunAsync

    #region CreateEvents

    static IEnumerable<SomethingHappened> CreateEvents(string streamId, int batchSize, int baseId)
    {
        foreach (var k in Enumerable.Range(0, batchSize - 5))
        {
            int id = baseId + k;
            var e = new SomethingHappened(id, $"Person {id}");
            yield return e;
        }
    }

    #endregion //  CreateEvents

    #region CreateFaultEvents

    static IEnumerable<FaultOccurred> CreateFaultEvents(string streamId, int batchSize, int baseId)
    {
        for (int i = 0; i < 5; i++)
        {
            int id = baseId + i;
            var e = new FaultOccurred(id, $"Person {id}", Environment.TickCount % 100);
            yield return e;
        }
    }

    #endregion //  CreateFaultEvents
}
