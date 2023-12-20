using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using Eventualize.Core;
using Eventualize.Core.Tests;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;
using static Eventualize.Core.Tests.TestHelper;



namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;


public sealed class SQLServerStorageAdapterTests : IDisposable
{
    private readonly SQLServerAdapterTestWorld _world;
    public EventualizeStorageContext _contextId = EventualizeStorageContext.CreateUnique();

    private readonly IConfigurationRoot _configuration;
    private readonly ITestOutputHelper _testLogger;

    public SQLServerStorageAdapterTests(ITestOutputHelper testLogger)
    {
        _testLogger = testLogger;
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();


        _world = new SQLServerAdapterTestWorld(_configuration, testLogger);
        Task t = _world.StorageMigration.CreateTestEnvironmentAsync();
        t.Wait();
    }

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
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, false);
        AssertAggregateStoredSuccessfully.assert(_world, aggregate, false);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithPendingEventsWithSnapshot_Succeed()
    {
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, true);
        AssertAggregateStoredSuccessfully.assert(_world, aggregate, true);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithStaleState_ThrowException()
    {
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await _world.StorageAdapter.SaveAsync(aggregate, true);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await _world.StorageAdapter.SaveAsync(aggregate, true));
    }

    [Fact(Skip = "not active")]
    public async Task SQLStorageAdapter_WhenStoringAggregateWithFutureState_ThrowException()
    {
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        IAsyncEnumerable<EventualizeEvent> events =
                            aggregate.PendingEvents.ToAsync();
        var aggregate2 =
            await aggregate.CreateAsync(events);
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

        AggregateParameter parameter = new(aggregate.Id, aggregate.Type);
        var latestSnapshot = await _world.StorageAdapter.TryGetSnapshotAsync<TestState>(parameter);
        Assert.NotNull(latestSnapshot);
        Assert.Equal(aggregate.State, latestSnapshot.Snapshot);
        Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, latestSnapshot.SnapshotSequenceId);
    }

    [Fact]
    public async Task SQLStorageAdapter_WhenGettingStoredEvents_Succeed()
    {
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(_world.StorageAdapter);

        AggregateSequenceParameter parameter = new(aggregate);

        var asyncEvents = _world.StorageAdapter.GetAsync(parameter);
        Assert.NotNull(asyncEvents);
        var events = await asyncEvents.ToEnumerableAsync();
        Assert.True(aggregate.PendingEvents.SequenceEqual(events));
    }
}
