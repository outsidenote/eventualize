// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

public class MongoDBStreamStructuresTests : StreamStructuresBaseTests
{
    public MongoDBStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => throw new NotImplementedException();

}