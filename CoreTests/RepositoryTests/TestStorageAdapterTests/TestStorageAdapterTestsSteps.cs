using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using CoreTests.AggregateTypeTests;
using CoreTests.AggregateTests;
using Core.Repository;
using System.Text.Json;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    public static class TestStorageAdapterTestsSteps
    {
        public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents()
        {
            var aggregate = TestAggregateConfigs.GetTestAggregate();
            for (int i = 0; i < 3; i++)
                aggregate.AddPendingEvent(await Event.EventTypeTests.GetCorrectTestEvent());
            return aggregate;

        }

        public static async Task<Aggregate<TestState>> PrepareAggregateWithPendingEvents(int? minEventsBetweenSnapshots)
        {
            var aggregate = TestAggregateConfigs.GetTestAggregate(new(), minEventsBetweenSnapshots);
            for (int i = 0; i < 3; i++)
                aggregate.AddPendingEvent(await Event.EventTypeTests.GetCorrectTestEvent());
            return aggregate;

        }
        public static async Task<Aggregate<TestState>> PrepareAggregateWithEvents()
        {
            List<Core.EventEntity> events = new();
            for (int i = 0; i < 3; i++)
                events.Add(await Event.EventTypeTests.GetCorrectTestEvent());
            return TestAggregateConfigs.GetTestAggregate(events, true);
        }

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, Aggregate<TestState> aggregate, List<Core.EventEntity>? events)
        {
            Assert.IsNotNull(events);
            List<Core.EventEntity>? storedEvents;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out storedEvents))
                throw new KeyNotFoundException(key);
            Assert.IsNotNull(storedEvents);
            Assert.AreEqual(events.Count, storedEvents.Count);
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, Aggregate<TestState> aggregate)
        {
            StoredSnapshotData<JsonDocument>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.IsNotNull(storedSnapshot);
            Assert.IsNotNull(storedSnapshot.Snapshot);
            Assert.AreEqual(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.Snapshot)));
        }

    }
}