// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using EvDb.UnitTests;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;

[Trait("DB", "MongoDB")]
public class MongoDBStreamSimpleTests : StreamSimpleBaseTests
{
    public MongoDBStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    #region GetOutboxName

    public string GetOutboxName(EvDbShardName shardName)
    {
        string schema = StorageContext.Schema.HasValue
            ? $"{StorageContext.Schema}."
            : string.Empty;
        string collectionPrefix = $"{schema}{StorageContext.ShortId}";
        var outboxCollectionFormat = $$"""{{collectionPrefix}}{0}_outbox""";
        if (string.Compare(shardName.Value, "outbox", true) == 0)
            shardName = string.Empty;
        string collectionName = string.Format(outboxCollectionFormat, shardName);
        return collectionName;
    }

    #endregion //  GetOutboxName

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);

    [Fact]
    public async Task Stream_Change_Stream_Succeed()
    {
        var startTime = new BsonTimestamp((int)(DateTime.UtcNow
                                            .AddSeconds(-2)
                                            .Subtract(new DateTime(1970, 1, 1)))
                                            .TotalSeconds, 1);

        await base.Stream_Basic_Succeed();

        string connectionString = _configuration.GetConnectionString("EvDbMongoDBConnection") ?? throw new EntryPointNotFoundException("EvDbMongoDBConnection");

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(StorageContext.DatabaseName);

        string outboxName = GetOutboxName(OutboxShards.Messaging);
        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(outboxName);
        using var cts = new CancellationTokenSource(Debugger.IsAttached ? TimeSpan.FromMinutes(15) : TimeSpan.FromSeconds(10));

        var options = new ChangeStreamOptions
        {
            StartAtOperationTime = startTime
        };
        var changes = WatchCollectionChangesAsync(collection, options, cts.Token).Take(6);
        await foreach (var change in changes)
        {
            _output.WriteLine($"Change detected: {change.OperationType} - {change.FullDocument}");
        }
    }


    private async IAsyncEnumerable<ChangeStreamDocument<BsonDocument>> WatchCollectionChangesAsync(
                                IMongoCollection<BsonDocument> collection,
                                ChangeStreamOptions options,
                                CancellationToken cancellationToken = default)
    {
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>()
            .Match(change =>
                    change.OperationType == ChangeStreamOperationType.Insert);
        options.FullDocument = ChangeStreamFullDocumentOption.UpdateLookup;

        using var cursor = await collection.WatchAsync(pipeline, options, cancellationToken);

        // Continue iterating the cursor until cancellation is requested
        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (ChangeStreamDocument<BsonDocument> change in cursor.Current)
            {
                yield return change;
            }
        }
    }
}
