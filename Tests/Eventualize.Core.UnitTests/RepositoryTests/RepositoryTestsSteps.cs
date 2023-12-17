using Eventualize.Core;
using Eventualize.Core.Tests;

namespace CoreTests.RepositoryTests
{
    public class RepositoryTestsSteps
    {

        public TestStorageAdapter _storageAdapter = new();
        public async Task<Repository> PrepareTestRepositoryWithStoredAggregate(EventualizeAggregate<TestState>? aggregate)
        {
            if (aggregate != null)
                await _storageAdapter.SaveAsync(aggregate, true);
            return new Repository(_storageAdapter);
        }

        public void AssertFetchedAggregateIsCorrect(EventualizeAggregate<TestState>? expectedAggregate, EventualizeAggregate<TestState>? fetchedAggregate)
        {
            if (expectedAggregate == null && fetchedAggregate == null) return;
            Assert.NotNull(expectedAggregate);
            Assert.NotNull(fetchedAggregate);

            Assert.Equal(expectedAggregate.State, fetchedAggregate.State);
            Assert.Equal(expectedAggregate.Id, fetchedAggregate.Id);
            Assert.Empty(fetchedAggregate.PendingEvents);
            Assert.Equal(2, fetchedAggregate.LastStoredSequenceId);
        }

        public async Task AssertStoredAggregateIsCorrect(EventualizeAggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var aggregateTypeName = aggregate.AggregateType.Name;
            IAsyncEnumerable<EventualizeEvent>? eventsAsync = _storageAdapter.GetAsync(aggregateTypeName, aggregate.Id);
            //await foreach (var item in eventsAsync)
            //{

            //}
            var events = await eventsAsync.ToEnumerableAsync();
            Assert.Equal(3, events.Count);
            Assert.Equal(events.Count - 1, aggregate.LastStoredSequenceId);
            Assert.Empty(aggregate.PendingEvents);

            var snapshotData = await _storageAdapter.TryGetSnapshotAsync<TestState>(aggregateTypeName, aggregate.Id);
            Assert.Equal(!isSnapshotStored, snapshotData is null);
            if (isSnapshotStored)
            {
                long expectedLastStoredSequenceId = events.Count - 1;
                Assert.Equal(expectedLastStoredSequenceId, aggregate.LastStoredSequenceId);
            }
        }

    }
}