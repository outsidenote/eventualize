using CoreTests.EventualizeRepositoryTests.TestStorageAdapterTests;
using Eventualize.Core;
using Eventualize.Core.Tests;

namespace CoreTests.EventualizeRepositoryTests
{
    public class RepositoryTests
    {
        [Fact]
        public async Task EventualizeRepository_WhenGettingAggregate_Succeed()
        {
            var repoTestSteps = new EventualizeRepositoryTestsSteps();
            var aggregateFactory = TestAggregateFactoryConfigs.GetAggregateFactory();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            IEventualizeRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var fetchedAggregate = await repository.GetAsync(aggregateFactory, aggregate.StreamUri.StreamId);
            repoTestSteps.AssertFetchedAggregateIsCorrect(aggregate, fetchedAggregate);
        }
        [Fact]
        public async Task EventualizeRepository_WhenGettingDifferentAggregate_Succeed()
        {
            var repoTestSteps = new EventualizeRepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            IEventualizeRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var aggregateFactory2 = TestAggregateFactoryConfigs.GetAggregateFactory(true);
            var aggregate2 = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(true);
            var fetchedAggregate = await repository.GetAsync(aggregateFactory2, aggregate.StreamUri.StreamId);
            repoTestSteps.AssertFetchedAggregateStateIsCorrect(aggregate2, fetchedAggregate);
        }

        [Fact]
        public async Task EventualizeRepository_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var repoTestSteps = new EventualizeRepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(10);
            IEventualizeRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, false);
        }

        [Fact]
        public async Task EventualizeRepository_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var repoTestSteps = new EventualizeRepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(1);
            IEventualizeRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
        }

        [Fact]
        public async Task EventualizeRepository_WhenStoringStaleAggregate_ThrowException()
        {
            var repoTestSteps = new EventualizeRepositoryTestsSteps();
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents(3);
            IEventualizeRepository repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            await Assert.ThrowsAsync<OCCException<TestState>>(async () => await repository.SaveAsync(aggregate));
        }

    }
}