using EvDb.Scenes;
using EvDb.UnitTests;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;

public abstract class StreamFactoryBaseTests : BaseIntegrationTests
{
    protected StreamFactoryBaseTests(ITestOutputHelper output,
                                    StoreType storeType) : base(output, storeType)
    {
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithEvents_Succeed()
    {

        var stream = await StorageContext
                        .GivenFactoryForStoredStreamWithEvents(_output, _storeType, TestingStreamStore)
                        .WhenGetStreamAsync();

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
        IEvDbSchoolStreamFactory f = StorageContext.CreateFactory(_storeType, TestingStreamStore);
        var (_, streamId) = await StorageContext
                        .GivenFactoryForStoredStreamWithEvents(_output, _storeType, TestingStreamStore);

        string stream1Id = streamId + "-a";
        string stream2Id = streamId + "-b";
        await StorageContext.GivenSavedEventsAsync(_output, _storeType, TestingStreamStore, stream1Id);
        await StorageContext.GivenSavedEventsAsync(_output, _storeType, TestingStreamStore, stream2Id);


        IEvDbSchoolStream stream1 = await f.WhenGetStreamAsync(stream1Id);
        IEvDbSchoolStream stream2 = await f.WhenGetStreamAsync(stream2Id);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(stream1Id, stream1.StreamAddress.StreamId);
            Assert.Single(stream1.Views.StudentStats.Students);
            var student1 = stream1.Views.StudentStats.Students[0];
            Assert.Equal(180, student1.Sum);
            Assert.Equal(3, student1.Count);
            Assert.Equal(0, stream1.CountOfPendingEvents);
            Assert.Equal(180, stream1.Views.ALL.Sum);

            Assert.Equal(stream2Id, stream2.StreamAddress.StreamId);
            Assert.Single(stream2.Views.StudentStats.Students);
            var student2 = stream2.Views.StudentStats.Students[0];
            Assert.Equal(180, student2.Sum);
            Assert.Equal(3, student1.Count);
            Assert.Equal(0, stream2.CountOfPendingEvents);
            Assert.Equal(180, stream2.Views.ALL.Sum);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotAndAnd6Events_Succeed()
    {
        var stream = await StorageContext
                        .GivenFactoryForStoredStreamWithEvents(_output, _storeType, TestingStreamStore, numOfGrades: 6)
                        .WhenGetStreamAsync();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            Assert.Equal(7, stream.StoredOffset);
            var viewsMeta = stream.Views.ToMetadata();
            Assert.All(viewsMeta, v => Assert.Equal(7, v.StoreOffset));
            Assert.All(viewsMeta, v => Assert.Equal(7, v.MemoryOffset));

            Assert.Equal(630, stream.Views.ALL.Sum);
            Assert.Equal(6, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(630, studentStat.Sum);
            Assert.Equal(6, studentStat.Count);
        }
    }

    [Fact]
    public async Task StreamFactory_WhenInstantiatingWithSnapshotOnDifferentOffsetAndEvents_Succeed()
    {
        var stream = await StorageContext.GivenStreamRetrievedFromStoreWithDifferentSnapshotOffset(_output, _storeType, TestingStreamStore);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            var meta = stream.Views.ToMetadata();
            Assert.Equal(5, stream.StoredOffset);
            Assert.Equal(0, meta.First().StoreOffset);
            Assert.Equal(5, meta.Last().StoreOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(5, v.MemoryOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(5, v.MemoryOffset));

            Assert.Equal(210, stream.Views.ALL.Sum);
            Assert.Equal(4, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(210, studentStat.Sum);
            Assert.Equal(4, studentStat.Count);
        }
    }
}