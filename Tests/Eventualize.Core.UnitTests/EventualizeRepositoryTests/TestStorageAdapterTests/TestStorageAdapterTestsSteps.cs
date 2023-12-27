using Eventualize.Core;
using Eventualize.Core.Tests;
using System.Text.Json;
using static Eventualize.Core.Tests.TestAggregateConfigs;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.EventualizeRepositoryTests.TestStorageAdapterTests
{
    public static class TestStorageAdapterTestsSteps
    {
        public static EventualizeAggregate<TestState> PrepareAggregateWithPendingEvents(bool useFoldingLogic2 = false)
        {
            EventualizeAggregate<TestState> aggregate = GetTestAggregate(useFoldingLogic2);
            for (int i = 0; i < 3; i++)
            {
                IEventualizeEvent e = GetCorrectTestEvent();
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

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate, IEnumerable<IEventualizeEvent>? events)
        {
            Assert.NotNull(events);
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out var storedEvents))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedEvents);
            IEnumerable<IEventualizeEvent> sev = storedEvents;
            Assert.True(events.SequenceEqual(sev, EventualizeEventComparer.Default));
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, EventualizeAggregate<TestState> aggregate)
        {
            EventualizeStoredSnapshot<JsonElement>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate.SnapshotUri);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedSnapshot);
            Assert.Equal(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.State)));
        }

        public static void AssertSnapshotFetchedSuccessfully<T>(EventualizeAggregate<T> expectedFromAggregate, EventualizeStoredSnapshot<T>? snapshot)
            where T : notnull, new()
        {
            Assert.NotNull(snapshot);
            Assert.Equal(expectedFromAggregate.State, snapshot.State);
            Assert.Equal(expectedFromAggregate.LastStoredOffset + expectedFromAggregate.PendingEvents.Count, snapshot.Cursor.Offset);
            Assert.Contains(expectedFromAggregate.SnapshotUri.ToString(), snapshot.Cursor.ToString());
        }

    }
}