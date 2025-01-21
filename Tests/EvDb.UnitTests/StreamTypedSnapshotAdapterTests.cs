namespace EvDb.Core.Tests;

using EvDb.Scenes;
using EvDb.UnitTests;
using FakeItEasy;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

public class StreamTypedSnapshotAdapterTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;
    private readonly TypedSnapshotFaker _typedSnapshotFaker = new TypedSnapshotFaker();

    public StreamTypedSnapshotAdapterTests(ITestOutputHelper output)
    {
        _output = output;
        A.CallTo(() => _storageAdapter.StoreStreamAsync(A<IImmutableList<EvDbEvent>>.Ignored, A<IImmutableList<EvDbMessage>>.Ignored, A<IEvDbStreamStoreData>.Ignored, A<CancellationToken>.Ignored))
            .Returns(StreamStoreAffected.Empty);
    }

    [Fact]
    public async Task Stream_WhenAddingPendingEvent_Succeed()
    {
        IEvDbSchoolStream stream = await _storageAdapter
                            .GivenLocalStreamWithPendingEvents(_output, typedStorageAdapter: _typedSnapshotFaker);

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            Assert.Equal(0, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(4, v.FoldOffset));

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
                            .GivenLocalStreamWithPendingEvents(_output, typedStorageAdapter: _typedSnapshotFaker)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithoutSnapshot(stream);

        void ThenStreamSavedWithoutSnapshot(IEvDbSchoolStream aggregate)
        {
            Assert.Equal(4, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(0, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(4, v.FoldOffset));

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
                            .GivenLocalStreamWithPendingEvents(_output, 6, typedStorageAdapter: _typedSnapshotFaker)
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot(stream);

        void ThenStreamSavedWithSnapshot(IEvDbSchoolStream stream)
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.FoldOffset));

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
                            .GivenLocalStreamWithPendingEvents(_output, typedStorageAdapter: _typedSnapshotFaker)
                            .GivenStreamIsSavedAsync()
                            .GivenAddingPendingEventsAsync()
                            .WhenStreamIsSavedAsync();

        ThenStreamSavedWithSnapshot(stream);

        void ThenStreamSavedWithSnapshot(IEvDbSchoolStream aggregate)
        {
            Assert.Equal(7, stream.StoredOffset);
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.StoreOffset));
            Assert.All(stream.Views.ToMetadata(), v => Assert.Equal(7, v.FoldOffset));

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
                    .GivenStreamWithStaleEvents(_output, typedStorageAdapter: _typedSnapshotFaker);

        await Assert.ThrowsAsync<OCCException>(async () => await stream.StoreAsync(default));
    }

    private class TypedSnapshotFaker : IEvDbStorageSnapshotAdapter<StudentStatsState>
    {
        private readonly ConcurrentDictionary<EvDbStreamAddress, EvDbStoredSnapshotData<StudentStatsState>> _states = new();

        public async Task<EvDbStoredSnapshot<StudentStatsState>> GetSnapshotAsync(
            EvDbViewAddress viewAddress, 
            CancellationToken cancellation = default)
        {
            await Task.Yield();
            if (_states.TryGetValue(viewAddress, out var state))
            {
                return new EvDbStoredSnapshot<StudentStatsState>(state.Offset, state.State);
            }
            return EvDbStoredSnapshot<StudentStatsState>.Empty;
        }
        public async Task StoreSnapshotAsync(
            EvDbStoredSnapshotData<StudentStatsState> data, CancellationToken cancellation = default)
        {
            await Task.Yield();
            _states.AddOrUpdate(data, data, (k, v) => data);
        }
    }

}