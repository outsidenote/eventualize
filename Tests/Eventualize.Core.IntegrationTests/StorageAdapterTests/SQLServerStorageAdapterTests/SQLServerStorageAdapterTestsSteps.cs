using Eventualize.Core;
using Eventualize.Core.Tests;
using System.Data;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests
{
    public static class SQLServerStorageAdapterTestsSteps
    {
        public static async Task<EventualizeAggregate<TestState>> StoreAggregateTwice(IEventualizeStorageAdapter storageAdapter)
        {
            EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
            await storageAdapter.SaveAsync(aggregate, true);
            var aggregate2 = await EventualizeAggregate.CreateAsync(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents.ToAsync());
            foreach (var pendingEvet in aggregate.PendingEvents)
                aggregate2.AddPendingEvent(pendingEvet);
            await storageAdapter.SaveAsync(aggregate2, true);
            return aggregate2;
        }

    }

}