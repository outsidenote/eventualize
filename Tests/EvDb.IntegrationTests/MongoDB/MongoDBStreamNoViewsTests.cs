// Ignore Spelling: Sql Mongo

namespace EvDb.Core.Tests;

using EvDb.Core.Adapters;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "MongoDB")]
public class MongoDBStreamNoViewsTests : StreamNoViewsBaseTests
{
    public MongoDBStreamNoViewsTests(ITestOutputHelper output) :
        base(output, StoreType.MongoDB)
    {
    }

    #region GetOutboxName

    public string GetOutboxName(EvDbShardName shardName)
    {
        string schema = StorageContext.Schema.HasValue
            ? $"{StorageContext.Schema}."
            : string.Empty;
        string collectionPrefix = $"{schema}{StorageContext.ShortId}";
        var outboxCollectionFormat = $$"""{{collectionPrefix}}{0}_outbox""";
        if (string.Compare(shardName.Value, "outbox", true) == 0)
            shardName = string.Empty;
        string collectionName = string.Format(outboxCollectionFormat, shardName);
        return collectionName;
    }

    #endregion //  GetOutboxName

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => StorageContext.GetOutboxFromMongoDBAsync(shard);
}
