using EvDb.Core;
using EvDb.Core.Tests;

namespace CoreTests.EvDbRepositoryTests
{
    public class EvDbRepositoryTestsSteps
    {

        public IEvDbStorageAdapter _storageAdapter = new TestStorageAdapter();
        public async Task<EvDbRepository> PrepareTestRepositoryWithStoredAggregate(EvDbAggregate<TestState>? aggregate)
        {
            if (aggregate != null)
                await _storageAdapter.SaveAsync(aggregate, true);
            return new EvDbRepository(_storageAdapter);
        }

        public void AssertFetchedAggregateIsCorrect(EvDbAggregate<TestState>? expectedAggregate, EvDbAggregate<TestState>? fetchedAggregate)
        {
            if (expectedAggregate == null && fetchedAggregate == null) return;
            Assert.NotNull(expectedAggregate);
            Assert.NotNull(fetchedAggregate);

            Assert.Equal(expectedAggregate.State, fetchedAggregate.State);
            Assert.Equal(expectedAggregate.StreamId, fetchedAggregate.StreamId);
            Assert.Empty(fetchedAggregate.PendingEvents);
            Assert.Equal(2, fetchedAggregate.LastStoredOffset);
        }

        public void AssertFetchedAggregateStateIsCorrect(EvDbAggregate<TestState>? expectedAggregate, EvDbAggregate<TestState>? fetchedAggregate)
        {
            Assert.NotNull(expectedAggregate);
            Assert.NotNull(fetchedAggregate);

            Assert.Equal(expectedAggregate.State, fetchedAggregate.State);
        }

        public async Task AssertStoredAggregateIsCorrect(EvDbAggregate<TestState> aggregate, bool isSnapshotStored)
        {
            EvDbStreamCursor streamCursor = new(aggregate.StreamId);
            IAsyncEnumerable<IEvDbStoredEvent>? eventsAsync = _storageAdapter.GetAsync(streamCursor);
            var events = await eventsAsync.ToEnumerableAsync();
            Assert.Equal(3, events.Count);
            Assert.Equal(events.Count - 1, aggregate.LastStoredOffset);
            Assert.Empty(aggregate.PendingEvents);


            var snapshot = await _storageAdapter.TryGetSnapshotAsync<TestState>(aggregate.SnapshotId);
            Assert.Equal(!isSnapshotStored, snapshot is null);
            if (isSnapshotStored)
            {
                long expectedLastStoredOffset = events.Count - 1;
                Assert.Equal(expectedLastStoredOffset, aggregate.LastStoredOffset);
            }
        }

    }
}