
using System.Text.Json;

namespace EvDb.Core;

public interface IEvDbViewFactory
{
    string ViewName { get; }

    IEvDbViewStore CreateEmpty(EvDbStreamAddress address,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null);

    Task<IEvDbViewStore> GetAsync(
        EvDbViewAddress address,
        JsonSerializerOptions? options,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default);
}

//public interface IEvDbViewFactory: IEvDbViewFactoryBase
//{
//    IEvDbViewStore CreateFromSnapshot(
//        EvDbStreamAddress address,
//        EvDbStoredSnapshot snapshot,
//        JsonSerializerOptions? options,
//        TimeProvider? timeProvider = null);

//    //IEvDbStorageSnapshotAdapter StoreAdapter { get; }
//}

//public interface IEvDbViewFactory<TState>: IEvDbViewFactoryBase
//{
//    IEvDbViewStore CreateFromSnapshot(
//        EvDbStreamAddress address,
//        EvDbStoredSnapshot<TState> snapshot,
//        JsonSerializerOptions? options,
//        TimeProvider? timeProvider = null);

//   // IEvDbStorageSnapshotAdapter<TState> StoreAdapter { get; }
//}
