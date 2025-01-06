
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbViewFactory
{
    string ViewName { get; }

    IEvDbViewStore CreateEmpty(EvDbStreamAddress address,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null);

    IEvDbViewStore CreateFromSnapshot(EvDbStreamAddress address,
        EvDbStoredSnapshot snapshot,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null);

    IEvDbStorageSnapshotAdapter StoreAdapter { get; }
}
