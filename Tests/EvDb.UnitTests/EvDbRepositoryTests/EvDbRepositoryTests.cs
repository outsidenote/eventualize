using CoreTests.EvDbRepositoryTests.TestStorageAdapterTests;
using EvDb.Core;
using EvDb.Core.Tests;

namespace CoreTests.EvDbRepositoryTests;

public class RepositoryTests
{
    [Fact(Skip = "Move to integration tests")]
    public async Task EvDbRepository_WhenGettingDifferentAggregate_Succeed()
    {
        throw new NotImplementedException();
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
        //var aggregateFactory2 = TestAggregateFactoryConfigs.GetAggregateFactory(true);
        //var aggregate2 = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(true);
        //var fetchedAggregate = await repository.GetAsync(aggregateFactory2, aggregate.StreamId.StreamId);
        //repoTestSteps.AssertFetchedAggregateStateIsCorrect(aggregate2, fetchedAggregate);
    }


    [Fact(Skip = "TBD")]
    public async Task EvDbRepository_WhenStoringAggregateWithSnapshot_Succeed()
    {
        throw new NotImplementedException();
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(1);
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
        //await repository.SaveAsync(aggregate);
        //await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
    }

    [Fact(Skip = "TBD")]
    public async Task EvDbRepository_WhenStoringStaleAggregate_ThrowException()
    {
        throw new NotImplementedException();
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
        //await Assert.ThrowsAsync<OCCException>(async () => await repository.SaveAsync(aggregate));
    }

}