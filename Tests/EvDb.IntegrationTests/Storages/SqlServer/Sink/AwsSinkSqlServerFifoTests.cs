// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "SqlServer:sink")]
public class AwsSinkSqlServerFifoTests: AwsSinkBaseTests
{
    public AwsSinkSqlServerFifoTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}