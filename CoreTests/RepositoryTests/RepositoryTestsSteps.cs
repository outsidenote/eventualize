using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;
using CoreTests.AggregateTypeTests;

namespace CoreTests.RepositoryTests
{
    public class RepositoryTestsSteps
    {

        public TestStorageAdapter storageAdapter = new();
        public async Task<Repository> PrepareTestRepositoryWithStoredAggregate(Aggregate<TestState>? aggregate)
        {
            if (aggregate != null)
                await storageAdapter.Store(aggregate, true);
            return new Repository(storageAdapter);
        }

        public void AssertFetchedAggregateIsCorrecrt(Aggregate<TestState>? expectedAggregate, Aggregate<TestState>? fetchedAggregate)
        {
            if (expectedAggregate == null && fetchedAggregate == null) return;
            Assert.IsNotNull(expectedAggregate);
            Assert.IsNotNull(fetchedAggregate);

            Assert.AreEqual(expectedAggregate.State, fetchedAggregate.State);
            Assert.AreEqual(expectedAggregate.Id, fetchedAggregate.Id);
            Assert.AreEqual(0, fetchedAggregate.PendingEvents.Count);
            Assert.AreEqual(2, fetchedAggregate.LastStoredSequenceId);
        }

        public async Task AssertStoredAggregateIsCorrect(Aggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var aggregateTypeName = aggregate.AggregateType.Name;
            var events = await storageAdapter.GetStoredEvents(aggregateTypeName, aggregate.Id, 0);
            Assert.AreEqual(3, events.Count);
            var snapshotData = await storageAdapter.GetLatestSnapshot<TestState>(aggregateTypeName, aggregate.Id);
            Assert.AreEqual(!isSnapshotStored, snapshotData is null);
            Assert.AreEqual(0, aggregate.PendingEvents.Count);
        }

    }
}