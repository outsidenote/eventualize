﻿// Ignore Spelling: Mongo
// Ignore Spelling: Admin

using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EvDb.Adapters.Store.MongoDB;

public sealed class MongoDBStorageAdmin : IEvDbStorageAdmin
{
    private readonly CollectionsSetup _collectionsSetup;
    private readonly StorageFeatures _features;
    private readonly EvDbShardName[] _shardNames;
    private bool _disposed;

    public MongoDBStorageAdmin(
        ILogger logger,
        MongoClientSettings settings,
        EvDbStorageContext storageContext,
        StorageFeatures features,
        params EvDbShardName[] shardNames)
    {
        var client = new MongoClient(settings);
        _collectionsSetup = CollectionsSetup.Create(logger,
                                                    client,
                                                    storageContext,
                                                    EvDbMongoDBCreationMode.CreateIfNotExists);
        _features = features;
        _shardNames = shardNames;
    }

    EvDbMigrationQueryTemplates IEvDbStorageAdmin.Scripts => throw new NotImplementedException();

    #region CreateEnvironmentAsync

    async Task IEvDbStorageAdmin.CreateEnvironmentAsync(CancellationToken cancellation)
    {
        if (_features.HasFlag(StorageFeatures.Stream))
            await _collectionsSetup.CreateEventsCollectionAsync(cancellation);
        if (_features.HasFlag(StorageFeatures.Outbox))
        {
            foreach (var shardName in _shardNames)
            {
                await _collectionsSetup.CreateOutboxCollectionIfNotExistsAsync(shardName, cancellation);
            }
        }
        if (_features.HasFlag(StorageFeatures.Snapshot))
            await _collectionsSetup.CreateSnapshotsCollectionAsync(cancellation);
    }

    #endregion //  CreateEnvironmentAsync

    #region DestroyEnvironmentAsync

    async Task IEvDbStorageAdmin.DestroyEnvironmentAsync(CancellationToken cancellation)
    {
        IAsyncDisposable disposal = this;
        await disposal.DisposeAsync();
    }

    #endregion //  DestroyEnvironmentAsync

    #region Dispose Pattern

    void IDisposable.Dispose()
    {
        DisposeAction();
        GC.SuppressFinalize(this);
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        IAsyncDisposable collectionsSetup = _collectionsSetup;
        await collectionsSetup.DisposeAsync();
    }

    private void DisposeAction()
    {
        if (_disposed)
            return;
        _disposed = true;
        IDisposable collectionsSetup = _collectionsSetup;
        collectionsSetup.Dispose();
    }

    ~MongoDBStorageAdmin()
    {
        DisposeAction();
    }

    #endregion //  DisposeAction Pattern
}

