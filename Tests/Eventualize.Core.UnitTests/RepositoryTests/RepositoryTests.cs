using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Eventualize.Core;
using Eventualize.Core.Tests;

namespace CoreTests.RepositoryTests
{
    public class RepositoryTests
    {
        [Fact]
        public async Task Repository_WhenGettingAggregate_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            IRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var fetchedAggregate = await repository.GetAsync(aggregateFactory, aggregate.StreamUri.StreamId);
            repoTestSteps.AssertFetchedAggregateIsCorrect(aggregate, fetchedAggregate);
        }

        [Fact]
        public async Task Repository_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(10);
            IRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, false);
        }

        [Fact]
        public async Task Repository_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(1);
            IRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
        }

        [Fact]
        public async Task Repository_WhenStoringStaleAggregate_ThrowException()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
            IRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            await Assert.ThrowsAsync<OCCException<TestState>>(async () => await repository.SaveAsync(aggregate));
        }

    }
}