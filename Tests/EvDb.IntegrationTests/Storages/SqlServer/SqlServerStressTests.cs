// Ignore Spelling: Sql

using EvDb.Core.Adapters;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

[Trait("Kind", "Stress")]
[Trait("DB", "SqlServer:stress")]
public sealed class SqlServerStressTests : StressBaseTests
{
    #region Ctor

    public SqlServerStressTests(ITestOutputHelper output) : base(output, StoreType.SqlServer)
    {
    }

    #endregion //  Ctor

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);
}
