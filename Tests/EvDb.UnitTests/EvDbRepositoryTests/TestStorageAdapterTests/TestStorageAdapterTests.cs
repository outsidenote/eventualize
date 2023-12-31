using EvDb.Core;

namespace CoreTests.EvDbRepositoryTests.TestStorageAdapterTests;

public class TestStorageAdapterTests
{
    [Fact]
    public void TestStorageAdapter_WhenStoringAggregateWithoutSnapshot_Succeed()
    {
        var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        var pendingEvents = aggregate.PendingEvents;
        TestStorageAdapter testStorageAdapter = new();
        testStorageAdapter.StorePendingEvents(aggregate);
        TestStorageAdapterTestsSteps.AssertEventsAreStored(
                                            testStorageAdapter,
                                            aggregate,
                                            pendingEvents);
    }

    [Fact]
    public async Task TestStorageAdapter_WhenStoringAggregateWithSnapshot_Succeed()
    {
        var aggregate = TestStorageAdapterTestsSteps.PrepareAggregateWithEvents();
        var testStorageAdapter = new TestStorageAdapter();
        IEvDbStorageAdapter adapter = testStorageAdapter;
        var pendingEvents = aggregate.PendingEvents;
        await adapter.SaveAsync(aggregate, true);
        TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, pendingEvents);
        TestStorageAdapterTestsSteps.AssertSnapshotIsStored(testStorageAdapter, aggregate);
    }
}