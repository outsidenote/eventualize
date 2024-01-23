using FakeItEasy;
using Xunit.Abstractions;

namespace EvDb.Core.Tests;
public interface IFoo
{
    Task<Bar<T>?> Get<T>(int x, string? s = null);
}

public record Bar<T>(string Key, T Value);



public sealed class AggregateFactoryTests
{
    private readonly IEvDbStorageAdapter _storageAdapter = A.Fake<IEvDbStorageAdapter>();
    private readonly ITestOutputHelper _output;

    public AggregateFactoryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AggregateFactory_WhenGettingDifferentAggregate_Succeed()
    {
        throw new NotImplementedException();
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var stream = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(stream);
        //var aggregateFactory2 = TestAggregateFactoryConfigs.GetAggregateFactory(true);
        //var aggregate2 = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(true);
        //var fetchedAggregate = await repository.GetAsync(aggregateFactory2, stream.StreamId.StreamId);
        //repoTestSteps.AssertFetchedAggregateStateIsCorrect(aggregate2, fetchedAggregate);
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithEvents_Succeed()
    {
        var stream = await Steps
                        .GivenFactoryForStoredStreamWithEvents(_output, _storageAdapter)
                        .GivenNoSnapshot(_storageAdapter)
                        .WhenGetAggregateAsync();
        
        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentAvg = stream.State.First().Value.Sum;
            //Assert.Equal(180, studentAvg);
            //Assert.Equal(0, stream.EventsCount);
        }
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithSnapshotAndWithoutEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output, false);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(70, studentSum);
            //Assert.Equal(0, stream.EventsCount);
        }
    }

    [Fact]
    public async Task AggregateFactory_WhenInstantiatingWithSnapshotAndEvents_Succeed()
    {
        var aggregate = await _storageAdapter.GivenAggregateRetrievedFromStore(_output);

        ThenStoredEventsAddedSuccessfully();

        void ThenStoredEventsAddedSuccessfully()
        {
            throw new NotImplementedException();
            //Assert.Single(stream.State);
            //var studentSum = stream.State.First().Value.Sum;
            //Assert.Equal(250, studentSum);
            //Assert.Equal(0, stream.EventsCount);
        }
    }

    [Fact(Skip = "until multi folding")]
    public void AggregateFactory_WhenFoldingEvents_Succeed()
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