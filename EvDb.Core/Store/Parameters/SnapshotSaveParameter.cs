using System.Diagnostics;
using System.Text.Json;

namespace EvDb.Core;

[DebuggerDisplay("{StreamId}, {Partition}, {Offset}")]
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
                    IEvDbAggregateDeprecated<T> aggregate,
                    JsonSerializerOptions? options = null)
    {
        string payload = JsonSerializer.Serialize(aggregate.State, options);
        return new(
            aggregate.SnapshotId.Domain,
            aggregate.SnapshotId.Partition,
            aggregate.SnapshotId.StreamId,
            aggregate.SnapshotId.Kind,
            aggregate.LastStoredOffset + aggregate.EventsCount,
            payload
        );
    }
    //public static SnapshotSaveParameter Create<T>(
    //                EvDbCollectionMeta<T> aggregate,
    //                JsonTypeInfo<T> jsonTypeInfo) where T : notnull, new()
    //{
    //    return new(
    //        aggregate.SnapshotId.Domain,
    //        aggregate.SnapshotId.Partition,
    //        aggregate.SnapshotId.StreamId,
    //        aggregate.SnapshotId.Kind,
    //        aggregate.LastStoredOffset + aggregate.EventsCount,
    //        JsonSerializer.Serialize(aggregate.State, jsonTypeInfo)
    //    );
    //}
};
