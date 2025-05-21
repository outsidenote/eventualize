namespace EvDb.Core.Tests;

using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Xunit.Abstractions;
#pragma warning disable S125 // Sections of code should not be commented out

[DebuggerDisplay("{_storeType}")]
public abstract class BaseIntegrationTests : IAsyncLifetime
{
    protected readonly ITestOutputHelper _output;
    protected readonly StoreType _storeType;
    private readonly bool _seSeparateSnapshotContext;
    protected readonly ILogger _logger = A.Fake<ILogger>();

    protected EvDbStreamTestingStorage TestingStreamStore { get; } = new EvDbStreamTestingStorage();

    // protected readonly TestContainers _containers;

    protected BaseIntegrationTests(ITestOutputHelper output,
                                   StoreType storeType,
                                   bool seSeparateSnapshotContext = false)
    {
        _output = output;
        _storeType = storeType;
        _seSeparateSnapshotContext = seSeparateSnapshotContext;
        //_containers = new TestContainers();

        EvDbSchemaName schema = storeType switch
        {
            StoreType.SqlServer => "dbo",
            StoreType.Postgres => "public",
            StoreType.MongoDB => "default",
            _ => EvDbSchemaName.Empty
        };
        EvDbDatabaseName dbName = storeType switch
        {
            StoreType.SqlServer => "master",
            StoreType.Postgres => "tests",
            StoreType.MongoDB => "tests",
            _ => EvDbDatabaseName.Empty
        };
        var context = new EvDbTestStorageContext(schema, dbName);
        StorageContext = context;
        if (seSeparateSnapshotContext)
        {
            AlternativeContext = new EvDbStorageContext(dbName, context.Environment,
                                                $"{context.Prefix}_different",
                                                schema);
        }
    }

    public abstract IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard);


    public EvDbStorageContext StorageContext { get; }
    public EvDbStorageContext? AlternativeContext { get; }

    public virtual async Task InitializeAsync()
    {
        //await _containers.StartAllAsync(_storeType);
        var storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, _storeType, StorageContext,
                                                    OutboxShards.MessagingVip,
                                                    OutboxShards.Messaging,
                                                    OutboxShards.Commands,
                                                    EvDbNoViewsOutbox.DEFAULT_SHARD_NAME,
                                                    EvDbShardName.Default);
        IEvDbStorageAdmin? storageMigrationSnapshot = null;
        if (_seSeparateSnapshotContext)
        {
            storageMigrationSnapshot = StoreAdapterHelper.CreateStoreMigration(_logger, _storeType,
                                                            AlternativeContext,
                                                            EvDbShardName.Default);
        }

        await storageMigration.CreateEnvironmentAsync();
        await (storageMigrationSnapshot?.CreateEnvironmentAsync() ?? Task.CompletedTask);
    }

    public virtual Task DisposeAsync()
    {

        //await _containers.StopAllAsync();
        return Task.CompletedTask;
    }
}
