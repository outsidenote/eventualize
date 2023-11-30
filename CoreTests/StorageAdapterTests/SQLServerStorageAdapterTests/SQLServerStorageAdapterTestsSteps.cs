using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.StorageAdapters.SQLServerStorageAdapter;
using CoreTests.AggregateTypeTests;
using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests
{
    public static class SQLServerStorageAdapterTestsSteps
    {
        public static async Task<Aggregate<TestState>> StoreAggregateTwice(SQLServerStorageAdapter storageAdapter)
        {
            var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
            await storageAdapter.Store(aggregate, true);
            var aggregate2 = new Aggregate<TestState>(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents);
            foreach (var pendingEvet in aggregate.PendingEvents)
                aggregate2.AddPendingEvent(pendingEvet);
            await storageAdapter.Store(aggregate2, true);
            return aggregate2;
        }

        public static void AssertEventsAreEqual(List<Core.Event.Event> events1, List<Core.Event.Event> events2)
        {
            Assert.AreEqual(events1.Count, events2.Count);
            events1.Select((e, index) =>
            {
                Assert.AreEqual(e, events2[index]);
                return true;
            });
        }
    }

}