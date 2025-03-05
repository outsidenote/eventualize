// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

public class MongoDBStreamChaosTests : StreamTxSopeBaseTests
{
    public MongoDBStreamChaosTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => throw new NotImplementedException();

}