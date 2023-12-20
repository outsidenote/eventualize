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
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var fetchedAggregate = await repository.GetAsync(aggregate);
            repoTestSteps.AssertFetchedAggregateIsCorrect(aggregate, fetchedAggregate);
        }

        [Fact]
        public async Task Repository_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(10);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, false);
        }

        [Fact]
        public async Task Repository_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(1);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.SaveAsync(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
        }

        [Fact]
        public async Task Repository_WhenStoringStaleAggregate_ThrowException()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(3);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            await Assert.ThrowsAsync<OCCException<TestState>>(async () => await repository.SaveAsync(aggregate));
        }

    }
}