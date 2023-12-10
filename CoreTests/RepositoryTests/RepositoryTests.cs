using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Eventualize.Core.Repository;
using CoreTests.AggregateTypeTests;
using System.Diagnostics;

namespace CoreTests.RepositoryTests
{
    [TestClass]
    public class RepositoryTests
    {
        [TestMethod]
        public async Task Repository_WhenGettingAggregate_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var fetchedAggregate = await repository.Get(aggregate.AggregateType, aggregate.Id);
            repoTestSteps.AssertFetchedAggregateIsCorrecrt(aggregate, fetchedAggregate);
        }

        [TestMethod]
        public async Task Repository_WhenStoringAggregateWithoutSnapshot_Scceed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(10);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.Store(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, false);
        }

        [TestMethod]
        public async Task Repository_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(1);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.Store(aggregate);
            await repoTestSteps.AssertStoredAggregateIsCorrect(aggregate, true);
        }

        [ExpectedException(typeof(OCCException<TestState>))]
        [TestMethod]
        public async Task Respotory_WhenStoringStaleAggregate_ThrowException()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents(3);
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            await repository.Store(aggregate);
        }

    }
}