using Eventualize.Core;
using Eventualize.Core.Tests;

namespace CoreTests.RepositoryTests
{
    public class RepositoryTestsSteps
    {

        public TestStorageAdapter storageAdapter = new();
        public async Task<Repository> PrepareTestRepositoryWithStoredAggregate(Aggregate<TestState>? aggregate)
        {
            if (aggregate != null)
                await storageAdapter.SaveAsync(aggregate, true);
            return new Repository(storageAdapter);
        }

        public void AssertFetchedAggregateIsCorrect(Aggregate<TestState>? expectedAggregate, Aggregate<TestState>? fetchedAggregate)
        {
            if (expectedAggregate == null && fetchedAggregate == null) return;
            Assert.NotNull(expectedAggregate);
            Assert.NotNull(fetchedAggregate);

            Assert.Equal(expectedAggregate.State, fetchedAggregate.State);
            Assert.Equal(expectedAggregate.Id, fetchedAggregate.Id);
            Assert.Empty(fetchedAggregate.PendingEvents);
            Assert.Equal(2, fetchedAggregate.LastStoredSequenceId);
        }

        public async Task AssertStoredAggregateIsCorrect(Aggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var aggregateTypeName = aggregate.AggregateType.Name;
            var events = await storageAdapter.GetAsync(aggregateTypeName, aggregate.Id, 0);
            Assert.Equal(3, events.Count);
            Assert.Equal(events.Count - 1, aggregate.LastStoredSequenceId);
            Assert.Empty(aggregate.PendingEvents);

            var snapshotData = await storageAdapter.TryGetSnapshotAsync<TestState>(aggregateTypeName, aggregate.Id);
            Assert.Equal(!isSnapshotStored, snapshotData is null);
            if (isSnapshotStored)
            {
                long expectedLastStoredSequenceId = events.Count - 1;
                Assert.Equal(expectedLastStoredSequenceId, aggregate.LastStoredSequenceId);
            }
        }

    }
}