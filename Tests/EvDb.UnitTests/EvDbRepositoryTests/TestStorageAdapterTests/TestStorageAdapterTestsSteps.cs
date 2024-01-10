using EvDb.Core;
using EvDb.Core.Tests;
using System.Text.Json;

using static EvDb.Core.Tests.TestAggregateConfigs;
using static EvDb.Core.Tests.TestHelper;

namespace CoreTests.EvDbRepositoryTests.TestStorageAdapterTests
{
    public static class TestStorageAdapterTestsSteps
    {
        public static EvDbAggregate<TestState> PrepareAggregateWithPendingEvents(bool useFoldingLogic2 = false)
        {
            throw new NotImplementedException();
            //EvDbAggregate<TestState> aggregate = GetTestAggregate(useFoldingLogic2);
            //for (int i = 0; i < 3; i++)
            //{
            //    IEvDbEvent e = GetCorrectTestEvent();
            //    aggregate.AddEvent(e);
            //}
            //return aggregate;
        }

        public static EvDbAggregate<TestState> PrepareAggregateWithEvents(int? minEventsBetweenSnapshots)
        {
            var events = GetPendingEvents(3);
            return GetTestAggregate(events, minEventsBetweenSnapshots);
        }
        public static EvDbAggregate<TestState> PrepareAggregateWithEvents()
        {
            var events = GetPendingEvents(3);
            return GetTestAggregate(events);
        }

        public static void AssertEventsAreStored(TestStorageAdapter testStorageAdapter, EvDbAggregate<TestState> aggregate, IEnumerable<IEvDbEvent>? events)
        {
            Assert.NotNull(events);
            string key = TestStorageAdapter.GetKeyValue(aggregate);
            if (!testStorageAdapter.Events.TryGetValue(key, out var storedEvents))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedEvents);
            IEnumerable<IEvDbEvent> sev = storedEvents;
            Assert.True(events.SequenceEqual(sev, EvDbEventComparer.Default));
        }

        public static void AssertSnapshotIsStored(TestStorageAdapter testStorageAdapter, EvDbAggregate<TestState> aggregate)
        {
            EvDbStoredSnapshot<JsonElement>? storedSnapshot;
            string key = TestStorageAdapter.GetKeyValue(aggregate.SnapshotId);
            if (!testStorageAdapter.Snapshots.TryGetValue(key, out storedSnapshot))
                throw new KeyNotFoundException(key);
            Assert.NotNull(storedSnapshot);
            Assert.Equal(aggregate.State, JsonSerializer.Deserialize<TestState>(JsonSerializer.Serialize(storedSnapshot.State)));
        }

        public static void AssertSnapshotFetchedSuccessfully<T>(EvDbAggregate<T> expectedFromAggregate, EvDbStoredSnapshot<T>? snapshot)
            where T : notnull, new()
        {
            Assert.NotNull(snapshot);
            Assert.Equal(expectedFromAggregate.State, snapshot.State);
            Assert.Equal(expectedFromAggregate.LastStoredOffset + expectedFromAggregate.EventsCount, snapshot.Cursor.Offset);
            Assert.Contains(expectedFromAggregate.SnapshotId.ToString(), snapshot.Cursor.ToString());
        }

    }
}