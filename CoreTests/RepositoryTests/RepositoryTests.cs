using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Core.Repository;
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
            // while (!Debugger.IsAttached)
            // {
            //     Thread.Sleep(100);
            // }
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(aggregate);
            var fetchedAggregate = await repository.Get(aggregate.AggregateType, aggregate.Id);
            repoTestSteps.AssertFetchedAggregateIsCorrecrt(aggregate, fetchedAggregate);
        }

        public async Task Repository_WhenStoringAggregateWithoutSnapshot_Scceed()
        {
            var repoTestSteps = new RepositoryTestsSteps();
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithEvents();
            var repository = await repoTestSteps.PrepareTestRepositoryWithStoredAggregate(null);
            await repository.Store(aggregate);

        }

    }
}