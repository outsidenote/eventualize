using Eventualize.Core;
using Eventualize.Core.Tests;
using System.Text.Json;
using static Eventualize.Core.Tests.TestAggregateConfigs;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    public static class TestStorageAdapterTestsSteps
    {
        public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents()
        {
            EventualizeAggregate<TestState> aggregate = GetTestAggregate();
            for (int i = 0; i < 3; i++)
            {
                EventualizeEvent e = GetCorrectTestEvent();
                aggregate.AddPendingEvent(e);
            }
            return aggregate;
        }

        public static async Task<EventualizeAggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
        {
            EventualizeAggregate<TestState> aggregate = await GetTestAggregateAsync(AsyncEnumerable<EventualizeEvent>.Empty, minEventsBetweenSnapshots);
            for (int i = 0; i < 3; i++)
            {
                EventualizeEvent e = GetCorrectTestEvent();
                aggregate.AddPendingEvent(e);
            }
            return aggregate;

        }
        public static async Task<EventualizeAggregate<TestState>> PrepareAggregateWithEvents()
        {
            List<EventualizeEvent> events = new();
            for (int i = 0; i < 3; i++)
            {
                EventualizeEvent e = GetCorrectTestEvent();
                events.Add(e);
            }
            return await TestAggregateConfigs.GetTestAggregateAsync(events.ToAsync(), true);
        }

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate, IEnumerable<EventualizeEvent>? events)
        {
            Assert.NotNull(events);
            List<EventualizeEvent>? storedEvents;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out storedEvents))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedEvents);
            Assert.True(events.SequenceEqual(storedEvents));
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate)
        {
            EventualizeStoredSnapshotData<JsonDocument>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedSnapshot);
            Assert.NotNull(storedSnapshot.Snapshot);
            Assert.Equal(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.Snapshot)));
        }

    }
}