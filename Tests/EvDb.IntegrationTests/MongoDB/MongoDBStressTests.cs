using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public sealed class MongoDBStressTests : StressBaseTests
{
    #region Ctor

    public MongoDBStressTests(ITestOutputHelper output) : base(output, StoreType.MongoDB)
    {
    }

    #endregion //  Ctor

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}