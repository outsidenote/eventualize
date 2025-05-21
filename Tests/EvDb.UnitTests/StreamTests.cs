namespace EvDb.Core.Tests;

using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

public class StreamTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public StreamTests(ITestOutputHelper output)
    {
        _output = output;
        A.CallTo(() => _storageAdapter.StoreStreamAsync(A<IImmutableList<EvDbEvent>>.Ignored, A<IImmutableList<EvDbMessage>>.Ignored, A<CancellationToken>.Ignored))
        .Returns(StreamStoreAffected.Empty);

    }

    [Fact]
    public async Task Stream_WhenAddingPendingEvent_Succeed()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                            .GivenLocalStreamWithPendingEvents(_output);

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            Assert.Equal(0, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(4, v.MemoryOffset));

            Assert.Equal(180, stream.Views.ALL.Sum);
            Assert.Equal(3, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(180, studentStat.Sum);
            Assert.Equal(3, studentStat.Count);
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithoutSnapshotting_Succeed()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                            .GivenLocalStreamWithPendingEvents(_output)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithoutSnapshot(stream);

        void ThenStreamSavedWithoutSnapshot(IEvDbSchoolStream aggregate)
        {
            Assert.Equal(4, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(4, v.MemoryOffset));

            Assert.Equal(180, stream.Views.ALL.Sum);
            Assert.Equal(3, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(180, studentStat.Sum);
            Assert.Equal(3, studentStat.Count);
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithSnapshotting_Succeed()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                            .GivenLocalStreamWithPendingEvents(_output, 6)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot(stream);

        void ThenStreamSavedWithSnapshot(IEvDbSchoolStream stream)
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.MemoryOffset));

            Assert.Equal(630, stream.Views.ALL.Sum);
            Assert.Equal(6, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(630, studentStat.Sum);
            Assert.Equal(6, studentStat.Count);
        }
    }

    [Fact]
    public async Task Stream_WhenStoringWithSnapshottingWhenStoringTwice_Succeed()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                            .GivenLocalStreamWithPendingEvents(_output)
                            .GivenStreamIsSavedAsync()
                            .GivenAddingPendingEventsAsync()
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot(stream);

        void ThenStreamSavedWithSnapshot(IEvDbSchoolStream aggregate)
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.MemoryOffset));

            Assert.Equal(360, stream.Views.ALL.Sum);
            Assert.Equal(6, stream.Views.ALL.Count);
            Assert.Single(stream.Views.StudentStats.Students);

            StudentStats studentStat = stream.Views.StudentStats.Students[0];
            var student = Steps.CreateStudentEntity();
            Assert.Equal(student.Id, studentStat.StudentId);
            Assert.Equal(student.Name, studentStat.StudentName);
            Assert.Equal(360, studentStat.Sum);
            Assert.Equal(6, studentStat.Count);
        }
    }


    [Fact]
    public async Task Stream_WhenStoringStaleStream_ThrowException()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                    .GivenStreamWithStaleEvents(_output);

        await Assert.ThrowsAsync<OCCException>(async () => await stream.StoreAsync(default));
    }

    [Fact]
    public async Task Stream_ConcorrentAdds_Succeed()
    {
        int expected = 100_000;
        IEvDbSchoolStream stream = _storageAdapter
                            .GivenLocalStream("id123");
        var sync = new ManualResetEventSlim(false);
        var ab = new ActionBlock<CourseCreatedEvent>(async e =>
                                        {
                                            sync.Wait();
                                            await stream.AppendAsync(e);
                                        },
                                        new ExecutionDataflowBlockOptions
                                        {
                                            MaxDegreeOfParallelism = 1000,
                                            SingleProducerConstrained = true
                                        });
        for (int i = 0; i < expected; i++)
        {
            var e = new CourseCreatedEvent(i, $"name {i}", 1 % 50 * 10 + 4);
            ab.Post(e);
        }
        ab.Complete();
        sync.Set();
        await ab.Completion;
        Assert.Equal(expected, stream.CountOfPendingEvents);
    }
}