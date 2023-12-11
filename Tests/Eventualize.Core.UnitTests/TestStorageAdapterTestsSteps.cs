using Eventualize.Core;
using Eventualize.Core.Aggregate;
using Eventualize.Core.Repository;
using Eventualize.Core.Tests;
using System.Text.Json;
using static Eventualize.Core.Tests.TestAggregateConfigs;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    public static class TestStorageAdapterTestsSteps
    {
        public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents()
        {
            Aggregate<TestState> aggregate = GetTestAggregate();
            for (int i = 0; i < 3; i++)
                aggregate.AddPendingEvent(await GetCorrectTestEvent());
            return aggregate;
        }

        public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
        {
            Aggregate<TestState> aggregate = GetTestAggregate(new(), minEventsBetweenSnapshots);
            for (int i = 0; i < 3; i++)
                aggregate.AddPendingEvent(await GetCorrectTestEvent());
            return aggregate;

        }
        public static async Task<Aggregate<TestState>> PrepareAggregateWithEvents()
        {
            List<EventEntity> events = new();
            for (int i = 0; i < 3; i++)
                events.Add(await GetCorrectTestEvent());
            return TestAggregateConfigs.GetTestAggregate(events, true);
        }

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, Aggregate<TestState> aggregate, List<EventEntity>? events)
        {
            Assert.NotNull(events);
            List<EventEntity>? storedEvents;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out storedEvents))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedEvents);
            Assert.Equal(events.Count, storedEvents.Count);
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, Aggregate<TestState> aggregate)
        {
            StoredSnapshotData<JsonDocument>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedSnapshot);
            Assert.NotNull(storedSnapshot.Snapshot);
            Assert.Equal(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.Snapshot)));
        }

    }
}