using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace EvDb.Core;

[DebuggerDisplay("{EntityId}, {EntityType}, {Offset}")]
public readonly record struct SnapshotSaveParameter(
                    string Domain,
                    string EntityType,
                    string EntityId,
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
            aggregate.SnapshotId.Domain,
            aggregate.SnapshotId.EntityType,
            aggregate.SnapshotId.EntityId,
            aggregate.SnapshotId.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count,
            JsonSerializer.Serialize(aggregate.State, options)
        );
    }
    public static SnapshotSaveParameter Create<T>(
                    EvDbAggregate<T> aggregate,
                    JsonTypeInfo<T> jsonTypeInfo) where T : notnull, new()
    {
        return new(
            aggregate.SnapshotId.Domain,
            aggregate.SnapshotId.EntityType,
            aggregate.SnapshotId.EntityId,
            aggregate.SnapshotId.AggregateType,
            aggregate.LastStoredOffset + aggregate.PendingEvents.Count,
            JsonSerializer.Serialize(aggregate.State, jsonTypeInfo)
        );
    }
};
