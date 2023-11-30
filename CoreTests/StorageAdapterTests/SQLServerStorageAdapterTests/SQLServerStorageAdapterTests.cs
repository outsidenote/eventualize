using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.StorageAdapters.SQLServerStorageAdapter;
using Core.StorageAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Formats.Asn1;
using System.Diagnostics;
using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using CoreTests.RepositoryTests.TestStorageAdapterTests;
using Core.Aggregate;
using CoreTests.AggregateTypeTests;
using Core.Repository;



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
        await world.StorageAdapter.Store(aggregate, false);
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
        await world.StorageAdapter.Store(aggregate, true);
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
        await world.StorageAdapter.Store(aggregate, true);
        await world.StorageAdapter.Store(aggregate, true);
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
        await world.StorageAdapter.Store(aggregate2, true);
    }

    [TestMethod]
    public async Task SQLStorageAdapter_WhenGettingLastSnapshotId_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var aggregate = await TestStorageAdapterTestsSteps.PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.Store(aggregate, true);
        var aggregate2 = new Aggregate<TestState>(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents);
        foreach (var pendingEvet in aggregate.PendingEvents)
            aggregate2.AddPendingEvent(pendingEvet);
        await world.StorageAdapter.Store(aggregate2, true);
        var latestSnapshotSequenceId = await world.StorageAdapter.GetLastStoredSequenceId(aggregate2);
        var expectedSequenceId = aggregate2.LastStoredSequenceId + aggregate2.PendingEvents.Count;
        Assert.AreEqual(expectedSequenceId, latestSnapshotSequenceId);
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
