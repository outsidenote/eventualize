using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

[DebuggerDisplay("{StreamId}, {StreamType}, {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    string Domain,
                    string StreamType,
                    string StreamId,
                    string AggregateType,
                    long Offset,
                    // TODO: [bnaya 2023-12-20] use ISnapshotPayload
                    string Payload)
{
    public static SnapshotSaveParameter Create<T>(
                    EvDbAggregate<T> aggregate,
                    JsonSerializerOptions? options = null) where T : notnull, new()
    {
        return new(
            aggregate.SnapshotUri.Domain,
            aggregate.SnapshotUri.StreamType,
            aggregate.SnapshotUri.StreamId,
            aggregate.SnapshotUri.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count,
            JsonSerializer.Serialize(aggregate.State, options)
        );
    }
    public static SnapshotSaveParameter Create<T>(
                    EvDbAggregate<T> aggregate,
                    JsonTypeInfo<T> jsonTypeInfo) where T : notnull, new()
    {
        return new(
            aggregate.SnapshotUri.Domain,
            aggregate.SnapshotUri.StreamType,
            aggregate.SnapshotUri.StreamId,
            aggregate.SnapshotUri.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count,
            JsonSerializer.Serialize(aggregate.State, jsonTypeInfo)
        );
    }
};
