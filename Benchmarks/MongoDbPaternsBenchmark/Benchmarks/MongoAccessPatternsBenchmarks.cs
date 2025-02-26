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
    private IMongoDatabase _database;
    private IMongoCollection<BsonDocument> _collectionById; // using _id from EvDbStreamCursor.ToString()
    private IMongoCollection<BsonDocument> _collectionComposed; // using unique index

    private const string DatabaseName = "evdb-benchmark_db";
    private const string CollectionById = "evdb-benchmark-by-id";
    private const string CollectionComposed = "evdb-benchmark-composed";
    private const int INSERT_BATCH_SIZE = 100;
    private const int INSERT_TIMES = 10_000;
    private const int GET_BATCH_SIZE = 100;
    private const int GET_TIMES = 200;
    private const int REPORT_INSERT_CYCLE = 30;

    private int _iterationCount = -1;

    private IImmutableList<EvDbEvent> _events;

    private FindOptions<BsonDocument> GET_OPTIONS = new FindOptions<BsonDocument> { Limit = GET_BATCH_SIZE };

    [IterationSetup]
    public void IterationSetup()
    {
        // Clear both collections before each iteration.
        _collectionById.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        _collectionComposed.DeleteMany(Builders<BsonDocument>.Filter.Empty);

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
        _database = _client.GetDatabase(DatabaseName);

        // Get or create the collections for each access pattern.
        _collectionById = _database.GetCollection<BsonDocument>(CollectionById);
        _collectionComposed = _database.GetCollection<BsonDocument>(CollectionComposed);
        // Ensure collections exist by creating them if they don't.
        var existingCollections = _database.ListCollectionNames().ToList();
        if (!existingCollections.Contains(CollectionById))
        {
            _database.CreateCollection(CollectionById);
        }
        if (!existingCollections.Contains(CollectionComposed))
        {
            _database.CreateCollection(CollectionComposed);
        }

        // Create indexes for both collections: domain, partition, stream_id, offset.
        var indexKeys = Builders<BsonDocument>.IndexKeys
                            .Ascending("domain")
                            .Ascending("partition")
                            .Ascending("stream_id")
                            .Ascending("offset");

        _collectionById.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys));
        _collectionComposed.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(indexKeys,
                                                                new CreateIndexOptions { Unique = true }));


        // (Optional) For pattern2, you might create an additional unique index on a separate field.
        //_collectionComposed.Indexes.CreateOne(new CreateIndexModel<BsonDocument>(
        //   Builders<BsonDocument>.IndexKeys.Ascending("unique_id"),
        //   new CreateIndexOptions { Unique = true }));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        // Drop the database once the benchmarks have finished.
        _client.DropDatabase(DatabaseName);
    }

    //[Benchmark]
    public void ById_InsertMany()
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
            _collectionById.InsertMany(documents);
            if (i % REPORT_INSERT_CYCLE == 0)
                Console.Write(".");
        }
        Console.WriteLine();
    }

    //[Benchmark]
    public void Composed_InsertMany()
    {
        // Pattern2: Use a unique field (simulate unique index) instead of setting _id manually
        for (int i = 0; i < INSERT_TIMES; i++)
        {
            var documents = _events.Skip(INSERT_BATCH_SIZE * i)
                                   .Take(INSERT_BATCH_SIZE)
                                   .Select(ev => ev.ToBsonDocument());
            _collectionComposed.InsertMany(documents);
            if (i % REPORT_INSERT_CYCLE == 0)
                Console.Write(".");
        }
        Console.WriteLine();
    }

    [Benchmark(Baseline = true)]
    public async Task ById_GetBatch()
    {
        ById_InsertMany();

        // Use the current iteration count as the starting offset.
        int baseOffset = _iterationCount * INSERT_BATCH_SIZE * INSERT_TIMES;

        // Retrieve a batch of 100 events starting at the offset.
        for (int i = 0; i < GET_TIMES; i++)
        {
            int firstOffset = baseOffset + (i * GET_BATCH_SIZE);
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("domain", "testdomain"),
                Builders<BsonDocument>.Filter.Eq("partition", "testpartition"),
                Builders<BsonDocument>.Filter.Eq("stream_id", "teststream"),
                Builders<BsonDocument>.Filter.Gte("offset", firstOffset)
            );
            var cursor = await _collectionById.FindAsync(filter, GET_OPTIONS);
            var result = await cursor.ToListAsync();

            Assert.Equal(GET_BATCH_SIZE, result.Count);
            Assert.True(result.Select(m => m["offset"].ToInt32())
                              .SequenceEqual(Enumerable.Range(firstOffset, GET_BATCH_SIZE)));
        }
    }

    [Benchmark]
    public async Task Composed_GetBatch()
    {
        Composed_InsertMany();

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
            var cursor = await _collectionComposed.FindAsync(filter, GET_OPTIONS);
            var result = await cursor.ToListAsync();

            Assert.Equal(GET_BATCH_SIZE, result.Count);
            Assert.True(result.Select(m => m["offset"].ToInt32())
                              .SequenceEqual(Enumerable.Range(firstOffset, GET_BATCH_SIZE)));
        }
    }

    private EvDbEvent CreateTestEvent(int index)
    {
        // Create a sample test event.
        var streamCursor = new EvDbStreamCursor("testdomain", "testpartition", "teststream", index);
        var payload = Encoding.UTF8.GetBytes("{\"sample\":\"data\"}");
        return new EvDbEvent("TestEvent", DateTimeOffset.UtcNow, "benchmark", streamCursor, payload);
    }
}
