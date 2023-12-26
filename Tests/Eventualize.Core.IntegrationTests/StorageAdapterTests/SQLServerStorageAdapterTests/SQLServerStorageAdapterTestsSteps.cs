using Eventualize.Core;
using Eventualize.Core.Tests;
using static Eventualize.Core.Tests.TestHelper;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests
{
    public static class SQLServerStorageAdapterTestsSteps
    {
        public static async Task<EventualizeAggregate<TestState>> StoreAggregateTwice(IEventualizeStorageAdapter storageAdapter, bool useFoldingLogic2 = false)
        {
            EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents(useFoldingLogic2);
            await storageAdapter.SaveAsync(aggregate, true);
            var aggregate2 = PrepareAggregateWithPendingEvents(aggregate, useFoldingLogic2);
            await storageAdapter.SaveAsync(aggregate2, true);
            return aggregate2;
        }
    }

}