using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

[Trait("Kind", "Integration")]
[Trait("DB", "Postgres")]
public sealed class PostgresStreamFactoryTests : StreamFactoryBaseTests
{
    public PostgresStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.Postgres)
    {
    }


    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}