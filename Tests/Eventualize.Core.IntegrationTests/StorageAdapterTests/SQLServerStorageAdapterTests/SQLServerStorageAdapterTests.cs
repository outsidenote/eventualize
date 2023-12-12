using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using Eventualize.Core;
using Eventualize.Core.Tests;
using Microsoft.Extensions.Configuration;
using static Eventualize.Core.Tests.TestHelper;



namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;


public sealed class SQLServerStorageAdapterTests : IDisposable
{
    private readonly SQLServerAdapterTestWorld _world;
    public StorageAdapterContextId _contextId = new(Guid.NewGuid());

    private readonly IConfigurationRoot _configuration;

    public SQLServerStorageAdapterTests()
    {
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();


        _world = new SQLServerAdapterTestWorld(_configuration);
        Task t = _world.StorageMigration.CreateTestEnvironmentAsync();
        t.Wait();
    }

    // TODO: [bnaya 2023-12-11] check if xunit support IAsyncDisposable
    public void Dispose()
    {
        _world.StorageMigration.DestroyTestEnvironmentAsync().Wait();
    }


    [Fact]
    public void SQLStorageAdapter_WhenCreatingTestEnvironment_Succeed()
    {
        var command = AssertEnvironmentWasCreated.GetSqlCommand(_world);
        var reader = command.ExecuteReader();
        reader.Read();
        bool isEnvExist = reader.GetBoolean(0);
        Assert.True(isEnvExist);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithoutSnapshot_Succeed()
    {
        var aggregate = await PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, false);
        AssertAggregateStoredSuccessfully.assert(_world, aggregate, false);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithSnapshot_Succeed()
    {
        var aggregate = await PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, true);
        AssertAggregateStoredSuccessfully.assert(_world, aggregate, true);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithStaleState_ThrowException()
    {
        var aggregate = await PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, true);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await _world.StorageAdapter.SaveAsync(aggregate, true));
    }

    [Fact(Skip = "not active")]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithFutureState_ThrowException()
    {
        var aggregate = await PrepareAggregateWithPendingEvents();
        var aggregate2 = new Aggregate<TestState>(aggregate.AggregateType, aggregate.Id, aggregate.MinEventsBetweenSnapshots, aggregate.PendingEvents);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await _world.StorageAdapter.SaveAsync(aggregate2, true));
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingLastSnapshotId_Succeed()
    {
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(_world.StorageAdapter);
        var latestSnapshotSequenceId = await _world.StorageAdapter.GetLastSequenceIdAsync(aggregate);
        var expectedSequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
        Assert.Equal(expectedSequenceId, latestSnapshotSequenceId);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingLatestSnapshot_Succeed()
    {
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(_world.StorageAdapter);
        var latestSnapshot = await _world.StorageAdapter.TryGetSnapshotAsync<TestState>(aggregate.AggregateType.Name, aggregate.Id);
        Assert.NotNull(latestSnapshot);
        Assert.Equal(aggregate.State, latestSnapshot.Snapshot);
        Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, latestSnapshot.SnapshotSequenceId);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingStoredEvents_Succeed()
    {
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(_world.StorageAdapter);
        var events = await _world.StorageAdapter.GetAsync(aggregate.AggregateType.Name, aggregate.Id, aggregate.LastStoredSequenceId + 1);
        Assert.NotNull(events);
        SQLServerStorageAdapterTestsSteps.AssertEventsAreEqual(events, aggregate.PendingEvents);
    }
}
