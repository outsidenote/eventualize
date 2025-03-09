// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using EvDb.Scenes;
using System.Text.Json;
using Xunit.Abstractions;

public class MongoDBStreamTests : StreamBaseTests
{
    public MongoDBStreamTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}