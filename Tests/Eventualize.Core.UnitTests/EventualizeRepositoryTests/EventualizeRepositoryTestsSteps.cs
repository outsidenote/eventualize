using Eventualize.Core;
using Eventualize.Core.Tests;

namespace CoreTests.EventualizeRepositoryTests
{
    public class EventualizeRepositoryTestsSteps
    {

        public IEventualizeStorageAdapter _storageAdapter = new TestStorageAdapter();
        public async Task<EventualizeRepository> PrepareTestRepositoryWithStoredAggregate(EventualizeAggregate<TestState>? aggregate)
        {
            if (aggregate != null)
                await _storageAdapter.SaveAsync(aggregate, true);
            return new EventualizeRepository(_storageAdapter);
        }

        public void AssertFetchedAggregateIsCorrect(EventualizeAggregate<TestState>? expectedAggregate, EventualizeAggregate<TestState>? fetchedAggregate)
        {
            if (expectedAggregate == null && fetchedAggregate == null) return;
            Assert.NotNull(expectedAggregate);
            Assert.NotNull(fetchedAggregate);

            Assert.Equal(expectedAggregate.State, fetchedAggregate.State);
            Assert.Equal(expectedAggregate.StreamUri, fetchedAggregate.StreamUri);
            Assert.Empty(fetchedAggregate.PendingEvents);
            Assert.Equal(2, fetchedAggregate.LastStoredOffset);
        }

        public async Task AssertStoredAggregateIsCorrect(EventualizeAggregate<TestState> aggregate, bool isSnapshotStored)
        {
            EventualizeStreamCursor streamCursor = new(aggregate.StreamUri);
            IAsyncEnumerable<EventualizeEvent>? eventsAsync = _storageAdapter.GetAsync(streamCursor);
            var events = await eventsAsync.ToEnumerableAsync();
            Assert.Equal(3, events.Count);
            Assert.Equal(events.Count - 1, aggregate.LastStoredOffset);
            Assert.Empty(aggregate.PendingEvents);


            var snapshot = await _storageAdapter.TryGetSnapshotAsync<TestState>(aggregate.StreamUri);
            Assert.Equal(!isSnapshotStored, snapshot is null);
            if (isSnapshotStored)
            {
                long expectedLastStoredOffset = events.Count - 1;
                Assert.Equal(expectedLastStoredOffset, aggregate.LastStoredOffset);
            }
        }

    }
}