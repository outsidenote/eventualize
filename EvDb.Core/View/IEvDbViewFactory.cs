
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbViewFactory
{
    string ViewName { get; }

    IEvDbViewStore CreateEmpty(EvDbStreamAddress address, JsonSerializerOptions? options);

    IEvDbViewStore CreateFromSnapshot(EvDbStreamAddress address,
        EvDbStoredSnapshot snapshot,
        JsonSerializerOptions? options);

    IEvDbStorageSnapshotAdapter StoreAdapter { get; }
}
