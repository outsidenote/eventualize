using Eventualize.Core.Tests;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    public class TestStorageAdapterTests
    {
        [Fact]
        public async Task TestStorageAdapter_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var aggregate =  TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            TestStorageAdapter testStorageAdapter = new();
            var testEvents = await testStorageAdapter.StorePendingEvents(aggregate);
            TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, testEvents);
        }

        [Fact]
        public async Task TestStorageAdapter_WhenStoringAggregateWithSnapshot_Succeed()
        {
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithEvents();
            TestStorageAdapter testStorageAdapter = new();
            var testEvents = await testStorageAdapter.SaveAsync(aggregate, true);
            TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, testEvents);
            TestStorageAdapterTestsSteps.AssertSnapshotIsStored(testStorageAdapter, aggregate);
        }
    }
}