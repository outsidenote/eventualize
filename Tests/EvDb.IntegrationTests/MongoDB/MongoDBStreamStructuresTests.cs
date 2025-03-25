// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("DB", "MongoDB")]
public class MongoDBStreamStructuresTests : StreamStructuresBaseTests
{
    public MongoDBStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}