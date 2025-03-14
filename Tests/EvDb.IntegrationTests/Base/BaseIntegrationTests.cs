namespace EvDb.Core.Tests;

using EvDb.Core;
using EvDb.Core.Adapters;
using EvDb.UnitTests;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Xunit.Abstractions;

[DebuggerDisplay("{_storeType}")]
public abstract class BaseIntegrationTests : IAsyncLifetime
{
    private readonly IEvDbStorageAdmin _storageMigration;
    private readonly IEvDbStorageAdmin? _storageMigrationSnapshot;
    protected readonly ITestOutputHelper _output;
    protected readonly StoreType _storeType;
    protected readonly ILogger _logger = A.Fake<ILogger>();

    protected BaseIntegrationTests(ITestOutputHelper output, StoreType storeType,
        bool seSeparateSnapshotContext = false)
    {
        _output = output;
        _storeType = storeType;
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
        _storageMigration = StoreAdapterHelper.CreateStoreMigration(_logger, storeType, context,
                                                    OutboxShards.MessagingVip,
                                                    OutboxShards.Messaging,
                                                    OutboxShards.Commands,
                                                    EvDbShardName.Default);
        if (seSeparateSnapshotContext)
        {
            AlternativeContext = new EvDbStorageContext(dbName, context.Environment,
                                                $"{context.Prefix}_different",
                                                schema);
            _storageMigrationSnapshot = StoreAdapterHelper.CreateStoreMigration(_logger, storeType,
                                                            AlternativeContext,
                                                            EvDbShardName.Default);
        }
    }

    public abstract IAsyncEnumerable<EvDbMessageRecord> GetOutboxAsync(EvDbShardName shard);


    public EvDbStorageContext StorageContext { get; }
    public EvDbStorageContext? AlternativeContext { get; }

    public virtual async Task InitializeAsync()
    {
        await _storageMigration.CreateEnvironmentAsync();
        await (_storageMigrationSnapshot?.CreateEnvironmentAsync() ?? Task.CompletedTask);
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;
}
