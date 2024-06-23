using EvDb.Core;
using Microsoft.AspNetCore.Mvc;
using EvDb.MinimalStructure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;

namespace EvDb.StressTestsWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class StressController : ControllerBase
{
    private readonly ILogger<StressController> _logger;
    private readonly IEvDbDemoStreamFactory _factory;
    private readonly IEvDbStorageMigration _storageMigration;
    private const int REPORT_INTERVAL = 200;

    public StressController(
        ILogger<StressController> logger,
        IEvDbDemoStreamFactory factory,
        IEvDbStorageMigration storageMigration)
    {
        _logger = logger;
        _factory = factory;
        _storageMigration = storageMigration;
    }

    [HttpPost]
    public async Task PostAsync([FromBody] StressOptions options)
    {
        (int writeCycleCount,
        int streamsCount,
        int degreeOfParallelismPerStream,
        int batchSize) = options;

        try
        {
            try
            {
                await _storageMigration.CreateEnvironmentAsync();
            }
            catch { }

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
                            IEvDbDemoStream stream = await _factory.GetAsync(streamId);
                            var tasks = events.Select(async e => await stream.AddAsync(e));
                            IEvDbEventMeta[] es = await Task.WhenAll(tasks);
                            for (int q = 0; q < es.Length; q++)
                            {
                                var e = es[q];
                            }

                            try
                            {
                                var offset0 = stream.StoredOffset;
                                int affected = await stream.StoreAsync();
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
                            _logger.LogInformation($"Count: {counter}: Avg Duration (sec): {sw.ElapsedMilliseconds / REPORT_INTERVAL /1000.0:N3}");
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
                var stream = await _factory.GetAsync(streamId);

                if (expectedEventsCount - 1 != stream.StoredOffset)
                    throw new Exception("Invalid offset");
            }
        }
        finally
        {
            await _storageMigration.DestroyEnvironmentAsync();
        }
    }

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
}
