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
    public static SnapshotSaveParameter Create<T>(
                    IEvDbStreamStore streamStore,
                    IEvDbView<T> aggregate,
                    JsonSerializerOptions? options = null)
    {
        string payload = JsonSerializer.Serialize(aggregate.State, options);
        return new(
            aggregate.Address.Domain,
            aggregate.Address.Partition,
            aggregate.Address.StreamId,
            aggregate.Address.ViewName,
            streamStore.LastStoredOffset + streamStore.EventsCount,
            payload
        );
    }
    //public static SnapshotSaveParameter Create<T>(
    //                EvDbCollectionMeta<T> aggregate,
    //                JsonTypeInfo<T> jsonTypeInfo) where T : notnull, new()
    //{
    //    return new(
    //        aggregate.SnapshotId.Domain,
    //        aggregate.SnapshotId.PartitionAddress,
    //        aggregate.SnapshotId.StreamAddress,
    //        aggregate.SnapshotId.Kind,
    //        aggregate.LastStoredOffset + aggregate.EventsCount,
    //        JsonSerializer.Serialize(aggregate.State, jsonTypeInfo)
    //    );
    //}
};
