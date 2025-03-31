using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

[Trait("DB", "Postgres")]
public sealed class PostgresStressTests : StressBaseTests
{
    #region Ctor

    public PostgresStressTests(ITestOutputHelper output) : base(output, StoreType.Postgres)
    {
    }

    #endregion //  Ctor

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}