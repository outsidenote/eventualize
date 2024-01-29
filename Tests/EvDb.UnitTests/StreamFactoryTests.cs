using FakeItEasy;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;


public sealed class StreamFactoryTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public StreamFactoryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithEvents_Succeed()
    {
        var stream = await Steps
                        .GivenFactoryForStoredStreamWithEvents(_output, _storageAdapter)
                        .GivenNoSnapshot(_storageAdapter)
                        .WhenGetAggregateAsync();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Single(stream.Views.StudentStats);
            var studentAvg = stream.Views.StudentStats.First().Sum;
            Assert.Equal(180, studentAvg);
            Assert.Equal(0, stream.CountOfPendingEvents);

            Assert.Equal(180, stream.Views.ALL.Sum);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenGettingDifferent_Succeed()
    {
        var (factory, streamId) = Steps
                        .GivenFactoryForStoredStreamWithEvents(_output, _storageAdapter);

        string stream1Id = streamId + "-a";
        string stream2Id = streamId + "-b";

        UnitTests.IEvDbSchoolStream stream1 = await (factory, stream1Id)
                         .GivenNoSnapshot(_storageAdapter)
                         .WhenGetAggregateAsync();
        var stream2 = await (factory, stream2Id)
                        .GivenNoSnapshot(_storageAdapter)
                        .WhenGetAggregateAsync();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(stream1Id, stream1.StreamAddress.StreamId);
            Assert.Single(stream1.Views.StudentStats);
            var studentAvg1 = stream1.Views.StudentStats.First().Sum;
            Assert.Equal(180, studentAvg1);
            Assert.Equal(0, stream1.CountOfPendingEvents);

            Assert.Equal(180, stream1.Views.ALL.Sum);

            Assert.Equal(stream2Id, stream2.StreamAddress.StreamId);
            Assert.Single(stream2.Views.StudentStats);
            var studentAvg2 = stream2.Views.StudentStats.First().Sum;
            Assert.Equal(180, studentAvg2);
            Assert.Equal(0, stream2.CountOfPendingEvents);

            Assert.Equal(180, stream2.Views.ALL.Sum);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output, false);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(70, studentSum);
            //Assert.Equal(0, stream.CountOfPendingEvents);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(250, studentSum);
            //Assert.Equal(0, stream.CountOfPendingEvents);
        }
    }

    [Fact(Skip = "until multi folding")]
    public void StreamFactory_WhenFoldingEvents_Succeed()
    {
        throw new NotImplementedException();

        //var events = TestAggregateConfigs.GetPendingEvents(3);
        //var (foldedState, count) = TestAggregateFactoryConfigs
        //    .GetAggregateFactory()
        //    .FoldingLogic
        //    .FoldEvents(events);
        //TestState expectedState = new TestState(3, 3, 30);
        //Assert.Equal(expectedState, foldedState);
        //Assert.Equal(3, count);
    }
}