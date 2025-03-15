// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

public class MongoDBStreamSimpleTests : StreamSimpleBaseTests
{
    public MongoDBStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}
