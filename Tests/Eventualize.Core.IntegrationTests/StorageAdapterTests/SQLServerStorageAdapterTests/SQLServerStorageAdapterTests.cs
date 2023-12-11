using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using Eventualize.Core;
using Eventualize.Core.Tests;
using Xunit.Abstractions;
using static Eventualize.Core.Tests.TestContext;
using static Eventualize.Core.Tests.TestHelper;



namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;

public sealed class SQLServerStorageAdapterTests : IDisposable
{
    public Dictionary<string, SQLServerAdapterTestWorld> Worlds = [];
#pragma warning disable S4487 // Unread "private" fields should be removed
    private readonly ITestOutputHelper _output;
#pragma warning restore S4487 // Unread "private" fields should be removed

    public SQLServerStorageAdapterTests(ITestOutputHelper output)
    {
        _output = output;
        BeforeEach().Wait();
    }

    public void Dispose() => AfterEach().Wait();

    private async Task BeforeEach()
    {
        var world = new SQLServerAdapterTestWorld();
        var name = TestName;
        Worlds.Add(name, world);
        await world.StorageAdapter.Init();
        await world.StorageAdapter.CreateTestEnvironment();
    }

    private async Task AfterEach()
    {
        var world = GetWorld();
        await world.StorageAdapter.DestroyTestEnvironment();
    }

    [Fact]
    public void SQLStorageAdapter_WhenCreatingTestEnvironment_Succeed()
    {
        var world = GetWorld();
        var command = AssertEnvironmentWasCreated.GetSqlCommand(world);
        var reader = command.ExecuteReader();
        reader.Read();
        bool isEnvExist = reader.GetBoolean(0);
        Assert.True(isEnvExist);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithoutSnapshot_Succeed()
    {
        var world = GetWorld();
        var aggregate = await PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, false);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, false);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithSnapshot_Succeed()
    {
        var world = GetWorld();
        var aggregate = await PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, true);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithStaleState_ThrowException()
    {
        var world = GetWorld();
        var aggregate = await PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await world.StorageAdapter.SaveAsync(aggregate, true));
    }

    [Fact(Skip = "not active")]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithFutureState_ThrowException()
    {
        var world = GetWorld();
        var aggregate = await PrepareAggregateWithPendingEvents();
        var aggregate2 = new Aggregate<TestState>(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await world.StorageAdapter.SaveAsync(aggregate2, true));
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingLastSnapshotId_Succeed()
    {
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var latestSnapshotSequenceId = await world.StorageAdapter.GetLastSequenceIdAsync(aggregate);
        var expectedSequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
        Assert.Equal(expectedSequenceId, latestSnapshotSequenceId);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingLatestSnapshot_Succeed()
    {
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var latestSnapshot = await world.StorageAdapter.TryGetSnapshotAsync<TestState>(aggregate.AggregateType.Name, aggregate.Id);
        Assert.NotNull(latestSnapshot);
        Assert.Equal(aggregate.State, latestSnapshot.Snapshot);
        Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, latestSnapshot.SnapshotSequenceId);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingStoredEvents_Succeed()
    {
        var world = GetWorld();
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var events = await world.StorageAdapter.GetAsync(aggregate.AggregateType.Name, aggregate.Id, aggregate.LastStoredSequenceId + 1);
        Assert.NotNull(events);
        SQLServerStorageAdapterTestsSteps.AssertEventsAreEqual(events, aggregate.PendingEvents);

    }


    private SQLServerAdapterTestWorld GetWorld()
    {
        if (!Worlds.TryGetValue(TestName, out var world))
            throw new KeyNotFoundException();
        return world;
    }

}
