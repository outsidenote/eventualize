//using EvDb.Core;
//using EvDb.Core.Tests;
//using static EvDb.Core.Tests.TestHelper;

//namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

//public static class SQLServerStorageAdapterTestsSteps
//{
//    public static async Task<EvDbAggregate<TestState>> StoreAggregateTwice(IEvDbStorageAdapter storageAdapter, bool useFoldingLogic2 = false)
//    {
//        EvDbAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents(useFoldingLogic2);
//        await storageAdapter.SaveAsync(aggregate, true);
//        var aggregate2 = PrepareAggregateWithPendingEvents(aggregate, useFoldingLogic2);
//        await storageAdapter.SaveAsync(aggregate2, true);
//        return aggregate2;
//    }
//}