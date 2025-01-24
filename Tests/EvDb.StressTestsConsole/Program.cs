using Cocona;
using Cocona.Builder;
using EvDb.Core;
using EvDb.StressTests;
using EvDb.StressTests.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;


(string? storeTypeArg, bool dbFound) = 
    args.Aggregate<string, (string? Db, bool Found)>((null, false), (acc, cur) =>
{
    if(acc.Found)
        return (cur, true);
    if (cur == "-d")
        return (null, true);
    return (null, false);
});
if (!dbFound)
    Console.WriteLine("Missing database type (`-d` switch)");
StoreType storeType = string.Compare(storeTypeArg, nameof(StoreType.SqlServer), true) == 0
    ? StoreType.SqlServer
    : StoreType.Posgres;


var context = new EvDbTestStorageContext(storeType);

var environmentName = Environment.GetEnvironmentVariable(
    "ASPNETCORE_ENVIRONMENT");

CoconaAppBuilder builder = CoconaApp.CreateBuilder();
builder.Logging.AddDebug();
builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true);
var services = builder.Services;
services.AddScoped<EvDbStorageContext>(_ => context);

services.AddEvDb()
      .AddDemoStreamFactory(c =>
      {
          switch (storeType)
          {
              case StoreType.SqlServer:
                  c.UseSqlServerStoreForEvDbStream();
                  break;
              case StoreType.Posgres:
                  c.UsePostgresStoreForEvDbStream();
                  break;
          }
      })
      .DefaultSnapshotConfiguration(c =>
      {
          switch (storeType)
          {
              case StoreType.SqlServer:
                  c.UseSqlServerForEvDbSnapshot();
                  break;
              case StoreType.Posgres:
                  c.UsePostgresForEvDbSnapshot();
                  break;
          }
      });
switch (storeType)
{
    case StoreType.SqlServer:
        services.AddEvDbSqlServerStoreMigration(OutboxShards.Table1, OutboxShards.Table2);
        break;
    case StoreType.Posgres:
        services.AddEvDbPostgresStoreMigration(OutboxShards.Table1, OutboxShards.Table2);
        break;
}
builder.AddOtel();

var app = builder.Build();
await app.RunAsync(async (
        ILogger<Program> logger,
        IEvDbDemoStreamFactory factory,
        IEvDbStorageMigration storageMigration,
        [Option('d', Description = $"Database type ({nameof(StoreType.SqlServer)}, {nameof(StoreType.Posgres)})")] StoreType _,
        [Option('w', Description = "Number of saving on the same stream (each save is saving a batch of events)")] int writeCycleCount = 3000,
        [Option('s', Description = "Number of independent streams, different streams doesn't collide with each other")] int streamsCount = 1,
        [Option('p', Description = "The degree of parallelism, this is what's cause the collision")] int degreeOfParallelismPerStream = 1,
        [Option('r', Description = "Report cycle")] int reportCycle = 200,
        [Option('o', Description = "Event % of producing outbox")] int outboxPercent = 200,
        [Option('b', Description = "Number of events to add in each batch")] int batchSize = 100) =>
{
    await storageMigration.CreateEnvironmentAsync();
    logger.LogInformation("DB Type = {type}", storeType);
    logger.LogInformation($"Total: {writeCycleCount * streamsCount}, Batch: {batchSize}, Parallel: {degreeOfParallelismPerStream}, Streams Count: {streamsCount}");
    var sw = Stopwatch.StartNew();
    int counter = 0;
    int occCounter = 0;
    int totalAffectedEvents = 0;
    int totalAffectedOutbox = 0;
    var tasks = Enumerable.Range(0, streamsCount)
        .Select(async i =>
        {
            var streamId = $"stream-{i}";

            int streamAffectedEvents = 0;
            int streamAffectedOutbox = 0;
            int occ = 0;

            var ab = new ActionBlock<int>(async j =>
            {
                var count = Interlocked.Increment(ref counter);
                bool success = false;
                IEnumerable<SomethingHappened> events = CreateEvents(batchSize, j * batchSize);
                IEnumerable<FaultOccurred> faultEvents = CreateFaultEvents(batchSize, j * batchSize);

                do
                {
                    IEvDbDemoStream stream = await factory.GetAsync(streamId);
                    var tasks = events.Select(async e => await stream.AddAsync(e));
                    await Task.WhenAll(tasks);
                    tasks = faultEvents.Select(async e => await stream.AddAsync(e));
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
                if (count % reportCycle == 0)
                {
                    sw.Stop();
                    double secound = sw.Elapsed.TotalSeconds;
                    double cyclePerSeconds = reportCycle / secound;
                    double eventsSeconds = streamAffectedEvents / secound;
                    double outboxSeconds = streamAffectedOutbox / secound;
                    double occSeconds = occ / secound;
                    Interlocked.Exchange(ref streamAffectedEvents, 0);
                    Interlocked.Exchange(ref streamAffectedOutbox, 0);
                    Interlocked.Exchange(ref occ, 0);
                    double perSeconds = reportCycle * batchSize / secound;
                    logger.LogInformation($"""
                                ------------------------------------------
                                Total Cycles: {count}
                                    events: {eventsSeconds:N0} per second, {totalAffectedEvents} total
                                    outbox: {outboxSeconds:N0} per second, {totalAffectedOutbox} total
                                    {cyclePerSeconds:N0} cycle per second
                                    {occSeconds:N0} OCC per seconds
                                    Validation!!: {perSeconds:N0} cycle * batch per seconds
                                """);
                    sw.Reset();
                    sw.Start();
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

#region CreateEvents

static IEnumerable<SomethingHappened> CreateEvents(int batchSize, int baseId)
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

static IEnumerable<FaultOccurred> CreateFaultEvents(int batchSize, int baseId)
{
    for (int i = 0; i < batchSize; i++)
    {
        int id = baseId + i;
        var e = new FaultOccurred(id, $"Person {id}", Environment.TickCount % 100);
        yield return e;
    }
}

#endregion //  CreateFaultEvents

