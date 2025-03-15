// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

public class PostgresStreamTxTests : StreamTxSopeBaseTests
{
    public PostgresStreamTxTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}