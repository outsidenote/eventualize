namespace EvDb.Core.Tests;

using EvDb.UnitTests;
using FakeItEasy;
using System.Text.Json;
using Xunit.Abstractions;
using STATE_TYPE = System.Collections.Immutable.IImmutableDictionary<int, EvDb.UnitTests.StudentStats>;

public class AggregateGenTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public AggregateGenTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Aggregate_WhenAddingPendingEvent_Succeed()
    {
        ISchoolStream aggregate = Steps
                            .GivenLocalAggerate(_output, _storageAdapter)
                            .WhenAddingPendingEvents();

        ThenPendingEventsAddedSuccessfully();

        void ThenPendingEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(aggregate.State);
            //var studentSum = aggregate.State.First().Value.Sum;
            //Assert.Equal(180, studentSum);
            //Assert.Equal(4, aggregate.EventsCount);
        }
    }


    [Fact]
    public async Task Aggregate_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output);
        aggregate.WhenAddGrades();

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(aggregate.State);
            //var studentSum = aggregate.State.First().Value.Sum;
            //Assert.Equal(430, studentSum);
            //Assert.Equal(3, aggregate.EventsCount);
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithoutSnapshot_Succeed()
    {
        ISchoolStream aggregate = await _storageAdapter.GivenLocalAggregateWithPendingEvents(_output)
                         .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithoutSnapshot(aggregate);

        void ThenAggregateSavedWithoutSnapshot(ISchoolStream aggregate)
        {
            Assert.Equal(0, aggregate.EventsCount);

            A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, false, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }


    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithSnapshot_Succeed()
    {
        ISchoolStream aggregate = await _storageAdapter.GivenLocalAggregateWithPendingEvents(_output)
                         .GivenAddGrades()
                         .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithSnapshot(aggregate);

        void ThenAggregateSavedWithSnapshot(ISchoolStream aggregate)
        {
            Assert.Equal(0, aggregate.EventsCount);

            A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, true, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringAggregateWithSnapshotWithOffset_Succeed()
    {
        ISchoolStream aggregate = await _storageAdapter.GivenStoredEvents(_output)
                        .GivenAddGradesAsync()
                        .WhenAggregateIsSavedAsync();

        ThenAggregateSavedWithSnapshot(aggregate);

        void ThenAggregateSavedWithSnapshot(ISchoolStream aggregate)
        {
            Assert.Equal(0, aggregate.EventsCount);

            A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, true, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _storageAdapter.SaveAsync(A<IEvDbAggregateDeprecated<STATE_TYPE>>.Ignored, A<bool>.Ignored, A<JsonSerializerOptions>.Ignored, A<CancellationToken>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }
    }

    [Fact]
    public async Task Aggregate_WhenStoringStaleAggregate_ThrowException()
    {
        //ISchool aggregate = await _storageAdapter.GivenLocalAggregateWithPendingEvents(_output)

        //ISchool aggregate = SetupMockSaveThrowOcc
        throw new NotImplementedException("OCC");
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
        //await Assert.ThrowsAsync<OCCException>(async () => await repository.SaveAsync(aggregate));
    }


}