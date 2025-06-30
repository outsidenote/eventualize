// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("Kind", "Integration:otel")]
[Trait("DB", "Postgres:otel")]
public class PostgresStreamOtelTests : StreamOtelBaseTests
{
    public PostgresStreamOtelTests(ITestOutputHelper output) :
        base(output, StoreType.Postgres)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) =>
                                RelationalOutboxTestHelper.GetOutboxAsync(_storeType, StorageContext, shard);

}