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
    public void SQLStorageAdapterTests_WhenCreatingTestEnvironment_Succeed()
    {
        // while (!Debugger.IsAttached)
        // {
        //     Thread.Sleep(100);
        // }
        var world = GetWorld();
        var command = AssertEnvironmentWasCreated.GetSqlCommand(world);
        var reader = command.ExecuteReader();
        reader.Read();
        bool isTestOk = reader.GetBoolean(0);
        Assert.AreEqual(true, isTestOk);
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
