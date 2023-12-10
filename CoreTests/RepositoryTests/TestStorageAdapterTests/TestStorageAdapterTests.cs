using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreTests.AggregateTests;
using Eventualize.Core.Aggregate;
using CoreTests.Event;
using CoreTests.AggregateTypeTests;
using CoreTests.RepositoryTests;
using System.Diagnostics;

namespace CoreTests.RepositoryTests.TestStorageAdapterTests
{
    [TestClass]
    public class TestStorageAdapterTests
    {
        [TestMethod]
        public async Task TestStorageAdapter_WhenStoringAggregateWithoutSnapshot_Succeed()
        {
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            TestStorageAdapter testStorageAdapter = new();
            var testEvents = await testStorageAdapter.StorePendingEvents(aggregate);
            TestStorageAdapterTestsSteps.AssertEventsAreStored(testStorageAdapter, aggregate, testEvents);
        }
        
        [TestMethod]
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