// Ignore Spelling:  Aws

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("Kind", "Integration:sink")]
[Trait("DB", "SqlServer:sink")]
[Collection("Sink")]
public class AwsSinkSqlServerTests : AwsSinkBaseTests
{
    public AwsSinkSqlServerTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}