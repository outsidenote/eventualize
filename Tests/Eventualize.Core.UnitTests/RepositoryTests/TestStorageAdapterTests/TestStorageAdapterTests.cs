using Eventualize.Core;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    public class TestStorageAdapterTests
    {
        [Fact]
        public async Task TestStorageAdapter_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            TestStorageAdapter testStorageAdapter = new();
            var testEvents = await testStorageAdapter.StorePendingEvents(aggregate);
            TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, testEvents);
        }

        [Fact]
        public async Task TestStorageAdapter_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithEvents();
            var testStorageAdapter = new TestStorageAdapter();
            IEventualizeStorageAdapter adapter = testStorageAdapter;
            var testEvents = await adapter.SaveAsync(aggregate, true);
            TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, testEvents);
            TestStorageAdapterTestsSteps.AssertSnapshotIsStored(testStorageAdapter, aggregate);
        }
    }
}