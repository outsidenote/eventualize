using CoreTests.EvDbRepositoryTests.TestStorageAdapterTests;
using EvDb.Core;
using EvDb.Core.Tests;

namespace CoreTests.EvDbRepositoryTests;

public class RepositoryTests
{
    [Fact]
    public async Task EvDbRepository_WhenGettingAggregate_Succeed()
    {
        throw new NotImplementedException();
        //var repoTestSteps = new EvDbRepositoryTestsSteps();
        //var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
        //var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        //IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
        //var fetchedAggregate = await repository.GetAsync(aggregateFactory, aggregate.StreamId.StreamId);
        //repoTestSteps.AssertFetchedAggregateIsCorrect(aggregate, fetchedAggregate);
    }
    [Fact]
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

    [Fact]
    public async Task EvDbRepository_WhenStoringAggregateWithoutSnapshot_Succeed()
    {
        var repoTestSteps = new EvDbRepositoryTestsSteps();
        var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(10);
        IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
        await repository.SaveAsync(aggregate);
        await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, false);
    }

    [Fact]
    public async Task EvDbRepository_WhenStoringAggregateWithSnapshot_Succeed()
    {
        var repoTestSteps = new EvDbRepositoryTestsSteps();
        var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(1);
        IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
        await repository.SaveAsync(aggregate);
        await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
    }

    [Fact]
    public async Task EvDbRepository_WhenStoringStaleAggregate_ThrowException()
    {
        var repoTestSteps = new EvDbRepositoryTestsSteps();
        var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
        IEvDbRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
        await Assert.ThrowsAsync<OCCException>(async () => await repository.SaveAsync(aggregate));
    }

}