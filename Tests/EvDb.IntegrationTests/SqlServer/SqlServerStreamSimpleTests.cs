// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("DB", "SqlServer")]
public class SqlServerStreamSimpleTests : StreamSimpleBaseTests
{
    public SqlServerStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.SqlServer)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}