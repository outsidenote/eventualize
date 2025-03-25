using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

[Trait("DB", "MongoDB")]

public sealed class MongoDBStreamFactoryTests : StreamFactoryBaseTests
{
    public MongoDBStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.MongoDB)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}
