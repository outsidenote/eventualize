using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

[Trait("Kind", "Integration")]
[Trait("DB", "SqlServer")]
public sealed class SqlServerStreamFactoryTests : StreamFactoryBaseTests
{
    public SqlServerStreamFactoryTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}