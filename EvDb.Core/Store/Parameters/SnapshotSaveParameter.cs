using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{StreamAddress}, {PartitionAddress}, {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    string Domain,
                    string Partition,
                    string StreamId,
                    string AggregateType,
                    long Offset,
                    // TODO: [bnaya 2023-12-20] use ISnapshotPayload
                    string Payload)
{
    [Obsolete("Deprecated")]
    public static SnapshotSaveParameter Create<T>(
                    IEvDbStreamStoreData streamStore,
                    IEvDbView<T> view,
                    JsonSerializerOptions? options = null)
    {
        string payload = JsonSerializer.Serialize(view.State, options);
        return new(
            view.Address.Domain,
            view.Address.Partition,
            view.Address.StreamId,
            view.Address.ViewName,
            streamStore.LastStoredOffset + streamStore.EventsCount,
            payload
        );
    }
    //public static SnapshotSaveParameter Create<T>(
    //                EvDbCollectionMeta<T> view,
    //                JsonTypeInfo<T> jsonTypeInfo) where T : notnull, new()
    //{
    //    return new(
    //        view.SnapshotId.Domain,
    //        view.SnapshotId.PartitionAddress,
    //        view.SnapshotId.StreamAddress,
    //        view.SnapshotId.Kind,
    //        view.LastStoredOffset + view.EventsCount,
    //        JsonSerializer.Serialize(view.State, jsonTypeInfo)
    //    );
    //}
};
