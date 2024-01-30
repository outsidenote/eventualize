using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using System.Collections;
using System.Collections.Immutable;
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
            Assert.Single(stream.Views.StudentStats.Students);
            var studentAvg = stream.Views.StudentStats.Students[0].Sum;
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
            Assert.Single(stream1.Views.StudentStats.Students);
            var studentAvg1 = stream1.Views.StudentStats.Students[0].Sum;
            Assert.Equal(180, studentAvg1);
            Assert.Equal(0, stream1.CountOfPendingEvents);

            Assert.Equal(180, stream1.Views.ALL.Sum);

            Assert.Equal(stream2Id, stream2.StreamAddress.StreamId);
            Assert.Single(stream2.Views.StudentStats.Students);
            var studentAvg2 = stream2.Views.StudentStats.Students[0].Sum;
            Assert.Equal(180, studentAvg2);
            Assert.Equal(0, stream2.CountOfPendingEvents);

            Assert.Equal(180, stream2.Views.ALL.Sum);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        var stream = await _storageAdapter.GivenStreamRetrievedFromStore(
                                _output,
                                false);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(60, stream.StoreOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(60, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(60, v.LastFoldedOffset));

            Assert.Equal(200, stream.Views.ALL.Sum);
            Assert.Equal(100, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(70, studentStat.Sum);
            Assert.Equal(20, studentStat.Count);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var stream = await _storageAdapter.GivenStreamRetrievedFromStore(
                                _output,
                                true);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(63, stream.StoreOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(60, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(63, v.LastFoldedOffset));

            Assert.Equal(380, stream.Views.ALL.Sum);
            Assert.Equal(103, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(250, studentStat.Sum);
            Assert.Equal(23, studentStat.Count);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotOnDifferentOffsetAndEvents_Succeed()
    {
        var stream = await _storageAdapter.GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset(
                                _output,
                                true);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(63, stream.StoreOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(60, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(63, v.LastFoldedOffset));

            Assert.Equal(380, stream.Views.ALL.Sum);
            Assert.Equal(103, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(220, studentStat.Sum);
            Assert.Equal(23, studentStat.Count);
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