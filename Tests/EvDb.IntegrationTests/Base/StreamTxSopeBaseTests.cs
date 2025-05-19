// Ignore Spelling: Sql

namespace EvDb.Core.Tests;
using EvDb.UnitTests;
using System.Transactions;
using Xunit.Abstractions;

public abstract class StreamTxSopeBaseTests : BaseIntegrationTests
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
                                .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId)
                                .WhenStreamIsSavedAsync();
        } // rollback
        IEvDbSchoolStreamFactory factory = StorageContext.CreateFactory(_storeType, TestingStreamStore);
        var newStream = await factory.GetAsync(streamId);

        Assert.Equal(0, newStream.StoredOffset);
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(0, v.MemoryOffset));
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
                                .GivenLocalStreamWithPendingEvents(_storeType, TestingStreamStore, streamId: streamId)
                                .WhenStreamIsSavedAsync();

            tx.Complete();
        }
        IEvDbSchoolStreamFactory factory = StorageContext.CreateFactory(_storeType, TestingStreamStore);
        var newStream = await factory.GetAsync(streamId);

        Assert.Equal(stream.StoredOffset, newStream.StoredOffset);
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
        Assert.All(newStream.Views.ToMetadata(), v => Assert.Equal(stream.StoredOffset, v.MemoryOffset));
    }
}