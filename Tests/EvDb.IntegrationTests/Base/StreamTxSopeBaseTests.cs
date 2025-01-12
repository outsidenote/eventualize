// Ignore Spelling: Sql

namespace EvDb.Core.Tests;

using EvDb.Adapters.Store.Postgres;
using EvDb.Adapters.Store.SqlServer;
using EvDb.Core.Adapters;
using EvDb.Scenes;
using EvDb.UnitTests;
using System.Text.Json;
using System.Transactions;
using Xunit.Abstractions;

public abstract class StreamTxSopeBaseTests : IntegrationTests
{
    public StreamTxSopeBaseTests(ITestOutputHelper output, StoreType storeType) :
        base(output, storeType)
    {
    }

    [Fact]
    public async Task Stream_Tx_Rollback()
    {

        var streamId = Steps.GenerateStreamId();
        using (var tx = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(3),
                                         TransactionScopeAsyncFlowOption.Enabled))
        {
            await StorageContext
                                .GivenLocalStreamWithPendingEvents(_storeType, streamId: streamId)
                                .WhenStreamIsSavedAsync();
        }
        IEvDbSchoolStreamFactory factory = StorageContext.CreateFactory(_storeType);
        var newStream = await factory.GetAsync(streamId);

        Assert.Equal(-1, newStream.StoredOffset);
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(-1, v.StoreOffset));
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(-1, v.FoldOffset));
    }

    [Fact]
    public async Task Stream_Tx_Commit()
    {
        IEvDbSchoolStream stream;
        var streamId = Steps.GenerateStreamId();
        using (var tx = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(3),
                                         TransactionScopeAsyncFlowOption.Enabled))
        {
            stream = await StorageContext
                                .GivenLocalStreamWithPendingEvents(_storeType, streamId: streamId)
                                .WhenStreamIsSavedAsync();

            tx.Complete();
        }
        IEvDbSchoolStreamFactory factory = StorageContext.CreateFactory(_storeType);
        var newStream = await factory.GetAsync(streamId);

        Assert.Equal(stream.StoredOffset, newStream.StoredOffset);
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(-1, v.StoreOffset));
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(stream.StoredOffset, v.FoldOffset));
    }
}