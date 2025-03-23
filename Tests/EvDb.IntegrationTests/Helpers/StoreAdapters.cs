namespace EvDb.Core.Tests;

public record StoreAdapters(IEvDbStorageStreamAdapter Stream, IEvDbStorageSnapshotAdapter Snapshot);
