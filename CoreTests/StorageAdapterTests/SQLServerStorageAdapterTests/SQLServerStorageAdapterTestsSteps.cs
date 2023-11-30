using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.StorageAdapters.SQLServerStorageAdapter;
using CoreTests.AggregateTypeTests;
using CoreTests.RepositoryTests.TestStorageAdapterTests;

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

    }
}