using CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries;
using Eventualize.Core;
using Eventualize.Core.Tests;
using Microsoft.Extensions.Configuration;
using System.Data;
using Xunit.Abstractions;
using static Eventualize.Core.Tests.TestHelper;



namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests;


public sealed class StorageAdapterTests 
{
    public EventualizeStorageContext _contextId = EventualizeStorageContext.CreateUnique();

    private readonly IConfigurationRoot _configuration;
    private readonly ITestOutputHelper _testLogger;

    public StorageAdapterTests(ITestOutputHelper testLogger)
    {
        _testLogger = testLogger;
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }



    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async void StorageAdapter_WhenCreatingTestEnvironment_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        var command = AssertEnvironmentWasCreated.GetSqlCommand(world);
        var reader = command.ExecuteReader();
        reader.Read();
        bool isEnvExist = reader.GetBoolean(0);
        Assert.True(isEnvExist);
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenStoringAggregateWithPendingEventsWithoutSnapshot_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, false);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, false);
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenStoringAggregateWithPendingEventsWithSnapshot_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        AssertAggregateStoredSuccessfully.assert(world, aggregate, true);
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenStoringAggregateWithStaleState_ThrowException(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        await world.StorageAdapter.SaveAsync(aggregate, true);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await world.StorageAdapter.SaveAsync(aggregate, true));
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenStoringAggregateWithFutureState_ThrowException(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        EventualizeAggregate<TestState> aggregate = PrepareAggregateWithPendingEvents();
        IAsyncEnumerable<EventualizeEvent> events =
                            aggregate.PendingEvents.ToAsync();
        var aggregate2 =
            await aggregate.CreateAsync(events);
        await Assert.ThrowsAsync<OCCException<TestState>>(async () => await world.StorageAdapter.SaveAsync(aggregate2, true));
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenGettingLastSnapshotId_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);
        var latestSnapshotSequenceId = await world.StorageAdapter.GetLastSequenceIdAsync(aggregate);
        var expectedSequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
        Assert.Equal(expectedSequenceId, latestSnapshotSequenceId);
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenGettingLatestSnapshot_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);

        AggregateParameter parameter = new(aggregate.Id, aggregate.Type);
        var latestSnapshot = await world.StorageAdapter.TryGetSnapshotAsync<TestState>(parameter);
        Assert.NotNull(latestSnapshot);
        Assert.Equal(aggregate.State, latestSnapshot.Snapshot);
        Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, latestSnapshot.SnapshotSequenceId);
    }

    [Theory]
    [InlineData(TypeOfDb.SqlServer)]
    [InlineData(TypeOfDb.Postgress)]
    //[InlineData(TypeOfDb.MySql)]
    public async Task StorageAdapter_WhenGettingStoredEvents_Succeed(TypeOfDb dbtype)
    {
        var world = await TestWorld.CreateAsync(dbtype, _configuration, _testLogger);
        var aggregate = await SQLServerStorageAdapterTestsSteps.StoreAggregateTwice(world.StorageAdapter);

        AggregateSequenceParameter parameter = new(aggregate);

        var asyncEvents = world.StorageAdapter.GetAsync(parameter);
        Assert.NotNull(asyncEvents);
        var events = await asyncEvents.ToEnumerableAsync();
        Assert.True(aggregate.PendingEvents.SequenceEqual(events));
    }
}
