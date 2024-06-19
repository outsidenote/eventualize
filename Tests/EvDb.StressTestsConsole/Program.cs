using Cocona;
using Cocona.Builder;
using EvDb.Core;
using EvDb.MinimalStructure;
using EvDb.StressTests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks.Dataflow;

var context = new EvDbTestStorageContext();

var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

CoconaAppBuilder builder = CoconaApp.CreateBuilder();
builder.Logging.AddDebug();
builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true);
var services = builder.Services;
services.AddEvDbDemoStreamFactory();
services.AddEvDbSqlServerStoreMigration(context);
services.AddEvDbSqlServerStore(context);
builder.AddOtel();

var app = builder.Build();

await app.RunAsync(async (
        ILogger<Program> logger,
        IEvDbDemoStreamFactory factory,
        IEvDbStorageMigration storageMigration,
        [Option('w', Description = "Number of saving on the same stream (each save is saving a batch of events)")] int writeCycleCount = 100,
        [Option('s', Description = "Number of independent streams, different streams doesn't collide with each other")] int streamsCount = 1,
        [Option('p', Description = "The degree of parallelism, this is what's cause the collision")] int degreeOfParallelismPerStream = 10,
        [Option('b', Description = "Number of events to add in each batch")] int batchSize = 5) =>
{
    await storageMigration.CreateEnvironmentAsync();

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
                    IEvDbDemoStream stream = await factory.GetAsync(streamId);
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

    logger.LogInformation("=================== COMPLETE ==================");

    int expectedEventsCount = writeCycleCount * batchSize;
    logger.LogInformation($"count: {counter}");
    logger.LogInformation($"OCC count: {occCounter}");
    for (int i = 0; i < streamsCount; i++)
    {
        var streamId = $"stream-{i}";
        var stream = await factory.GetAsync(streamId);

        if (expectedEventsCount - 1 != stream.StoredOffset)
            throw new Exception("Invalid offset");
    }
});

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

