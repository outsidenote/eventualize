using Eventualize.Core;
using Eventualize.Core.Tests;
using System.Text.Json;
using static Eventualize.Core.Tests.TestAggregateConfigs;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.EventualizeRepositoryTests.TestStorageAdapterTests
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

        public static EventualizeAggregate<TestState> PrepareAggregateWithEvents(int? minEventsBetweenSnapshots)
        {
            var events = GetPendingEvents(3);
            return GetTestAggregate(events, minEventsBetweenSnapshots);
        }
        public static EventualizeAggregate<TestState> PrepareAggregateWithEvents()
        {
            var events = GetPendingEvents(3);
            return GetTestAggregate(events);
        }

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate, IEnumerable<EventualizeEvent>? events)
        {
            Assert.NotNull(events);
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out var storedEvents))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedEvents);
            Assert.True(
                events.SequenceEqual(storedEvents
                    .Select(x => new EventualizeEvent(
                        x.EventType,
                        x.CapturedAt,
                        x.CapturedBy,
                        x.JsonData
                    ))
                )
            );
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate)
        {
            EventualizeStoredSnapshot<JsonElement>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedSnapshot);
            Assert.Equal(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.State)));
        }

    }
}