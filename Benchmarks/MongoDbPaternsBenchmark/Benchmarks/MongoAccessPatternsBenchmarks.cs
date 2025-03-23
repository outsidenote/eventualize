using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Immutable;
using System.Text;
using Xunit;

namespace MongoBenchmark;

[SimpleJob(launchCount: 1, warmupCount: 1, iterationCount: 1, invocationCount: 128)]
//[MemoryDiagnoser]
[Config(typeof(ShortRunConfig))]
public class MongoAccessPatternsBenchmarks
{
    // MongoDB client, database and collections for each pattern.
    private MongoClient _client;
    private IMongoDatabase _db;
    private IMongoCollection<BsonDocument> _collectionById; // using _id from EvDbStreamCursor.ToString()
    private IMongoCollection<BsonDocument> _collectionCompund; // using unique index

    private const string DatabaseName = "evdb-benchmark_db";
    private const string CollectionById = "evdb-benchmark-by-id";
    private const string CollectionComposed = "evdb-benchmark-composed";
    private const int INSERT_BATCH_SIZE = 1000;
    private const int INSERT_TIMES = 50;
    private const int GET_BATCH_SIZE = 100;
    private const int GET_TIMES = 20;
    private const int REPORT_INSERT_CYCLE = 50;
    private TimeSpan Delay = TimeSpan.FromMilliseconds(450);

    private int _iterationCount = -1;

    private IImmutableList<EvDbEvent> _events;

    #region Setup/Cleanup

    [IterationSetup]
    public void IterationSetup()
    {
        // Clear both collections before each iteration.
        //_collectionById.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        //_collectionCompund.DeleteMany(Builders<BsonDocument>.Filter.Empty);

        // Increment the iteration counter for each iteration.
        var iteration = Interlocked.Increment(ref _iterationCount);
        int start = iteration * INSERT_BATCH_SIZE * INSERT_TIMES;
        _events = Enumerable.Range(start, INSERT_BATCH_SIZE * INSERT_TIMES).
                    Select(i => CreateTestEvent(i))
                    .ToImmutableArray();
    }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _client = new MongoClient("mongodb://localhost:27017");
        _client.DropDatabase(DatabaseName);
        _db = _client.GetDatabase(DatabaseName);

        // Get or create the collections for each access pattern.
        _collectionById = _db.GetCollection<BsonDocument>(CollectionById);
        _collectionCompund = _db.GetCollection<BsonDocument>(CollectionComposed);
        // Ensure collections exist by creating them if they don't.
        var existingCollections = _db.ListCollectionNames().ToList();
        if (!existingCollections.Contains(CollectionById))
            _db.CreateCollection(CollectionById);
        if (!existingCollections.Contains(CollectionComposed))
            _db.CreateCollection(CollectionComposed);

        // Create indexes for both collections: domain, partition, stream_id, offset.
        var indexKeys = Builders<BsonDocument>.IndexKeys
                            .Ascending("domain")
                            .Ascending("partition")
                            .Ascending("stream_id");
        var j = indexKeys.ToJson();
        var indexPrimaryKeys = indexKeys
                                .Ascending("offset");
        // var byIdOptions = new CreateIndexOptions { Name = "domain_partition_id" };
        // _collectionById.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys, byIdOptions));
        var compoundOptions = new CreateIndexOptions { Name = "domain_partition_id_offset", Unique = true };
        _collectionCompund.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexPrimaryKeys, compoundOptions));


        // (Optional) For pattern2, you might create an additional unique index on a separate field.
        //_collectionCompund.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
        //   Builders<BsonDocument>.IndexKeys.Ascending("unique_id"),
        //   new CreateIndexOptions { Unique = true }));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Drop the database once the benchmarks have finished.
        //_client.DropDatabase(DatabaseName);
    }

    #endregion //  Setup/Cleanup

    #region Compound

    //[Benchmark]
    public async Task Compound_InsertMany()
    {
        // Pattern2: Use a unique field (simulate unique index) instead of setting _id manually
        for (int i = 0; i < INSERT_TIMES; i++)
        {
            var documents = _events.Skip(INSERT_BATCH_SIZE * i)
                                   .Take(INSERT_BATCH_SIZE)
                                   .Select(ev => ev.ToBsonDocument());
            await _collectionCompund.InsertManyAsync(documents);
            if (i % REPORT_INSERT_CYCLE == 0)
                Console.Write(".");
        }
        Console.WriteLine();
    }

    [Benchmark]
    public async Task Compound_GetBatch()
    {
        await Compound_InsertMany();

        // Use the current iteration count as the starting offset.
        int baseOffset = _iterationCount * INSERT_BATCH_SIZE * INSERT_TIMES;

        for (int i = 0; i < GET_TIMES; i++)
        {
            int firstOffset = baseOffset + (i * GET_BATCH_SIZE);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("domain", "testdomain"),
                Builders<BsonDocument>.Filter.Eq("partition", "testpartition"),
                Builders<BsonDocument>.Filter.Eq("stream_id", "teststream"),
                Builders<BsonDocument>.Filter.Gte("offset", firstOffset)
            );
            var sorting = Builders<BsonDocument>.Sort.Ascending("domain")
                                                                          .Ascending("partition")
                                                                          .Ascending("stream_id")
                                                                          .Ascending("offset");
            var cursor = _collectionCompund.Find(filter)
                                            .Sort(sorting)
                                            .Limit(GET_BATCH_SIZE);
            var result = await cursor.ToListAsync();
            for (int j = 0; j < 10; j++)
            {
                if (result.Count == GET_BATCH_SIZE)
                    break;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Expected {GET_BATCH_SIZE} but got {result.Count}");
                Console.ResetColor();
                await Task.Delay(Delay);
            }

            Assert.Equal(result.Select(m => m["offset"].ToInt32()),
                        Enumerable.Range(firstOffset, GET_BATCH_SIZE));
        }
    }

    #endregion //  Compound

    #region ById

    //[Benchmark]
    public async Task ById_InsertMany()
    {
        for (int i = 0; i < INSERT_TIMES; i++)
        {
            var documents = _events.Skip(INSERT_BATCH_SIZE * i)
                                   .Take(INSERT_BATCH_SIZE)
                                   .Select(ev =>
            {
                var doc = ev.ToBsonDocument();
                // Set _id to the concatenated string (e.g., "domain:partition:stream:offset")
                doc["_id"] = ev.StreamCursor.ToString();
                return doc;
            });
            await _collectionById.InsertManyAsync(documents);
            if (i % REPORT_INSERT_CYCLE == 0)
                Console.Write(".");
        }
        Console.WriteLine();
    }

    [Benchmark(Baseline = true)]
    public async Task ById_GetBatch()
    {
        await ById_InsertMany();

        // Use the current iteration count as the starting offset.
        int baseOffset = _iterationCount * INSERT_BATCH_SIZE * INSERT_TIMES;

        for (int i = 0; i < GET_TIMES; i++)
        {
            int firstOffset = baseOffset + (i * GET_BATCH_SIZE);
            IFindFluent<BsonDocument, BsonDocument> query = CreateByIdQuery(firstOffset, i);
            var result = await query.ToListAsync();

            for (int j = 0; j < 10; j++)
            {
                if (result.Count == GET_BATCH_SIZE)
                    break;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Expected {GET_BATCH_SIZE} but got {result.Count}");
                Console.ResetColor();
                await Task.Delay(Delay);
            }
            Assert.Equal(result.Select(m => m["offset"].ToInt32()),
                        Enumerable.Range(firstOffset, GET_BATCH_SIZE));
        }
    }

    [Benchmark]
    public async Task ById_GetViaCursor()
    {
        ById_InsertMany();

        // Use the current iteration count as the starting offset.
        int baseOffset = _iterationCount * INSERT_BATCH_SIZE * INSERT_TIMES;

        for (int i = 0; i < GET_TIMES; i++)
        {
            int firstOffset = baseOffset + (i * GET_BATCH_SIZE);
            IFindFluent<BsonDocument, BsonDocument> query = CreateByIdQuery(firstOffset, i);
            using IAsyncCursor<BsonDocument> cursor = await query.ToCursorAsync();
            var result = new List<BsonDocument>();
            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    // Convert from BsonDocument back to EvDbEvent.
                    result.Add(doc);
                }
            }

            for (int j = 0; j < 10; j++)
            {
                if (result.Count == GET_BATCH_SIZE)
                    break;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Expected {GET_BATCH_SIZE} but got {result.Count}");
                Console.ResetColor();
                await Task.Delay(Delay);
            }

            Assert.Equal(result.Select(m => m["offset"].ToInt32()),
                        Enumerable.Range(firstOffset, GET_BATCH_SIZE));
        }
    }

    private IFindFluent<BsonDocument, BsonDocument> CreateByIdQuery(int firstOffset, int i)
    {
        var fromId = new EvDbStreamCursor("testdomain", "testpartition", "teststream", firstOffset).ToString();
        var startwithId = new EvDbStreamCursor("testdomain", "testpartition", "teststream", 0).ToNonOffsetString();
        var filter = Builders<BsonDocument>.Filter.And(
                                            Builders<BsonDocument>.Filter.Regex(
                                                "_id", new BsonRegularExpression($"^{startwithId}")),
                                            Builders<BsonDocument>.Filter.Gte(
                                                "_id", fromId));

        IFindFluent<BsonDocument, BsonDocument> query = _collectionById.Find(filter)
                                          .Sort(Builders<BsonDocument>.Sort.Ascending("_id"))
                                          .Limit(GET_BATCH_SIZE);
        return query;
    }


    #endregion //  ById

    private EvDbEvent CreateTestEvent(int index)
    {
        // Create a sample test event.
        var streamCursor = new EvDbStreamCursor("testdomain", "testpartition", "teststream", index);
        var payload = Encoding.UTF8.GetBytes("{\"sample\":\"data\"}");
        return new EvDbEvent("TestEvent", DateTimeOffset.UtcNow, "benchmark", streamCursor, payload);
    }
}
