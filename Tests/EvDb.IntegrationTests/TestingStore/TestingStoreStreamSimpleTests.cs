// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions;
using System.Collections.Generic;
using Xunit.Abstractions;

[Trait("Kind", "Integration")]
[Trait("DB", "TestingStreamStore")]
public class TestingStoreNoViewsTests : StreamNoViewsBaseTests
{
    public TestingStoreNoViewsTests(ITestOutputHelper output) :
        base(output, StoreType.Testing)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => TestingStreamStore.GetAllRecordMessagesAsync(shard);
}

[Trait("DB", "TestingStreamStore")]
public class TestingStoreStreamSimpleTests : StreamSimpleBaseTests
{
    public TestingStoreStreamSimpleTests(ITestOutputHelper output) :
        base(output, StoreType.Testing)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => TestingStreamStore.GetAllRecordMessagesAsync(shard);
}

[Trait("DB", "TestingStreamStore")]
public class TestingStoreStreamTests : StreamBaseTests
{
    public TestingStoreStreamTests(ITestOutputHelper output) :
        base(output, StoreType.Testing)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => TestingStreamStore.GetAllRecordMessagesAsync(shard);
}


[Trait("DB", "TestingStreamStore")]
public class TestingStoreStreamStructuresTests : StreamStructuresBaseTests
{
    public TestingStoreStreamStructuresTests(ITestOutputHelper output) :
        base(output, StoreType.Testing)
    {
    }

    public override IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard) => TestingStreamStore.GetAllRecordMessagesAsync(shard);
}
