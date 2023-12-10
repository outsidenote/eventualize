using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eventualize.Core.StorageAdapters.SQLServerStorageAdapter;
using Eventualize.Core.StorageAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Formats.Asn1;
using System.Diagnostics;
using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Eventualize.Core.Aggregate;
using CoreTests.AggregateTypeTests;
using Eventualize.Core.Repository;
using Azure.Identity;



namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

[TestClass]
public class SQLServerStorageAdapterTests
{
    public TestContext? TestContext { get; set; }

    public Dictionary<string, SQLServerAdapterTestWorld> Worlds = [];

    [TestInitialize]
    public async Task BeforeEach()
    {
        var world = new SQLServerAdapterTestWorld();
        Worlds.Add(GetTestName(), world);
        await world.StorageAdapter.Init();
        await world.StorageAdapter.CreateTestEnvironment();
    }

    [TestCleanup]
    public async Task AfterEach()
    {
        var world = GetWorld();
        await world.StorageAdapter.DestroyTestEnvironment();
    }

    [TestMethod]
    public void SQLStorageAdapter_WhenCreatingTestEnvironment_Succeed()
    {
        var world = GetWorld();
        var command = AssertEnvironmentWasCreated.GetSqlCommand(world);
        var reader = command.ExecuteReader();
        reader.Read();
        bool isEnvExist = reader.GetBoolean(0);
        Assert.AreEqual(true, isEnvExist);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithoutSnapshot_Succeed()
    {
        var world = GetWorld();
        var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, false);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, false);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithSnapshot_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, true);
    }

    [TestMethod]
    [ExpectedException(typeof(OCCException<TestState>))]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithStaleState_ThrowException()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        await world.StorageAdapter.SaveAsync(aggregate, true);
    }

    [Ignore]
    [TestMethod]
    [ExpectedException(typeof(OCCException<TestState>))]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithFutureState_ThrowException()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        var aggregate2 = new Aggregate<TestState>(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents);
        await world.StorageAdapter.SaveAsync(aggregate2, true);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenGettingLastSnapshotId_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var latestSnapshotSequenceId = await world.StorageAdapter.GetLastSequenceIdAsync(aggregate);
        var expectedSequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
        Assert.AreEqual(expectedSequenceId, latestSnapshotSequenceId);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenGettingLatestSnapshot_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var latestSnapshot = await world.StorageAdapter.TryGetSnapshotAsync<TestState>(aggregate.AggregateType.Name, aggregate.Id);
        Assert.IsNotNull(latestSnapshot);
        Assert.AreEqual(aggregate.State, latestSnapshot.Snapshot);
        Assert.AreEqual(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, latestSnapshot.SnapshotSequenceId);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenGettingStoredEvents_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var events = await world.StorageAdapter.GetAsync(aggregate.AggregateType.Name, aggregate.Id, aggregate.LastStoredSequenceId + 1);
        Assert.IsNotNull(events);
        SQLServerStorageAdapterTestsSteps.AssertEventsAreEqual(events,aggregate.PendingEvents);

    }

    private string GetTestName()
    {
        if (TestContext == null)
            throw new ArgumentNullException(nameof(TestContext));
        string? testName = TestContext.TestName;
        if (string.IsNullOrEmpty(testName))
            throw new ArgumentNullException(nameof(testName));
        return testName;
    }

    private SQLServerAdapterTestWorld GetWorld()
    {
        if (!Worlds.TryGetValue(GetTestName(), out var world))
            throw new KeyNotFoundException();
        return world;
    }

}
