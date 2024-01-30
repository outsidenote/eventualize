namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using System.Text.Json;
using Xunit.Abstractions;
using STATE_TYPE = EvDb.Scenes.StudentStats;

public class StreamTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public StreamTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Aggregate_WhenAddingPendingEvent_Succeed()
    {
        IEvDbSchoolStream aggregate = Steps
                            .GivenLocalAggerate(_output, _storageAdapter)
                            .WhenAddingPendingEvents();

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(180, studentSum);
            //Assert.Equal(4, stream.CountOfPendingEvents);
        }
    }


    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenStreamRetrievedFromStore(_output);
        aggregate.WhenAddGrades();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(430, studentSum);
            //Assert.Equal(3, stream.CountOfPendingEvents);
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithoutSnapshot_Succeed()
    {
        IEvDbSchoolStream aggregate = await _storageAdapter.GivenLocalStreamWithPendingEvents(_output)
                         .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithoutSnapshot(aggregate);

        void ThenAggregateSavedWithoutSnapshot(IEvDbSchoolStream aggregate)
        {
            throw new NotImplementedException();
            //Assert.Equal(0, stream.CountOfPendingEvents);

            //A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, false, A<Options>.Ignored, A<CancellationToken>.Ignored))
            //    .MustHaveHappenedOnceExactly();
        }
    }


    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithSnapshot_Succeed()
    {
        IEvDbSchoolStream aggregate = await _storageAdapter.GivenLocalStreamWithPendingEvents(_output)
                         .GivenAddGrades()
                         .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithSnapshot(aggregate);

        void ThenAggregateSavedWithSnapshot(IEvDbSchoolStream stream)
        {
            Assert.Equal(0, stream.CountOfPendingEvents);

            throw new NotImplementedException();
            //A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, true, A<Options>.Ignored, A<CancellationToken>.Ignored))
            //    .MustHaveHappenedOnceExactly();

            //A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<Options>.Ignored, A<CancellationToken>.Ignored))
            //    .MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithSnapshotWithOffset_Succeed()
    {
        IEvDbSchoolStream aggregate = await _storageAdapter.GivenStoredEvents(_output)
                        .GivenAddGradesAsync()
                        .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithSnapshot(aggregate);

        void ThenAggregateSavedWithSnapshot(IEvDbSchoolStream stream)
        {
            Assert.Equal(0, stream.CountOfPendingEvents);

            throw new NotImplementedException();
            //A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, true, A<Options>.Ignored, A<CancellationToken>.Ignored))
            //    .MustHaveHappenedOnceExactly();

            //A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<Options>.Ignored, A<CancellationToken>.Ignored))
            //    .MustHaveHappenedTwiceExactly();
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringStaleAggregate_ThrowException()
    {
        //ISchool stream = await _storageAdapter.GivenLocalStreamWithPendingEvents(_output)

        //ISchool stream = SetupMockSaveThrowOcc
        throw new NotImplementedException("OCC");
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var stream = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(stream);
        //await Assert.ThrowsAsync<OCCException>(async () => await repository.SaveAsync(stream));
    }


}