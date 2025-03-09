using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.EvDbMongoDB.Internals;

public static class EvDbFileds
{
    private static readonly Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

    #region Event

    public static class Event
    {
        public static readonly string Domain = toSnakeCase(nameof(EvDbEventRecord.Domain));
        public static readonly string Partition = toSnakeCase(nameof(EvDbEventRecord.Partition));
        public static readonly string StreamId = toSnakeCase(nameof(EvDbEventRecord.StreamId));
        public static readonly string Offset = toSnakeCase(nameof(EvDbEventRecord.Offset));
        public static readonly string EventType = toSnakeCase(nameof(EvDbEventRecord.EventType));
        public static readonly string Payload = toSnakeCase(nameof(EvDbEventRecord.Payload));
        public static readonly string CapturedAt = toSnakeCase(nameof(EvDbEventRecord.CapturedAt));
        public static readonly string CapturedBy = toSnakeCase(nameof(EvDbEventRecord.CapturedBy));
        public static readonly string TraceId = toSnakeCase(nameof(EvDbEventRecord.TraceId));
        public static readonly string SpanId = toSnakeCase(nameof(EvDbEventRecord.SpanId));
    }

    #endregion //  Event

    #region Outbox

    public static class Outbox
    {
        public static readonly string Domain = toSnakeCase(nameof(EvDbMessageRecord.Domain));
        public static readonly string Partition = toSnakeCase(nameof(EvDbMessageRecord.Partition));
        public static readonly string StreamId = toSnakeCase(nameof(EvDbMessageRecord.StreamId));
        public static readonly string Offset = toSnakeCase(nameof(EvDbMessageRecord.Offset));
        public static readonly string Channel = toSnakeCase(nameof(EvDbMessageRecord.Channel));
        public static readonly string MessageType = toSnakeCase(nameof(EvDbMessageRecord.MessageType));
        public static readonly string EventType = toSnakeCase(nameof(EvDbMessageRecord.EventType));
        public static readonly string ShardName = toSnakeCase(nameof(EvDbMessage.ShardName));
        public static readonly string Payload = toSnakeCase(nameof(EvDbMessageRecord.Payload));
        public static readonly string CapturedAt = toSnakeCase(nameof(EvDbMessageRecord.CapturedAt));
        public static readonly string CapturedBy = toSnakeCase(nameof(EvDbMessageRecord.CapturedBy));
        public static readonly string SerializeType = toSnakeCase(nameof(EvDbMessageRecord.SerializeType));
        public static readonly string TraceId = toSnakeCase(nameof(EvDbMessageRecord.TraceId));
        public static readonly string SpanId = toSnakeCase(nameof(EvDbMessageRecord.SpanId));
    }

    #endregion //  Outbox

    #region Snapshot

    public static class Snapshot
    {
        public static readonly string Domain = toSnakeCase(nameof(EvDbViewAddress.Domain));
        public static readonly string Partition = toSnakeCase(nameof(EvDbViewAddress.Partition));
        public static readonly string StreamId = toSnakeCase(nameof(EvDbViewAddress.StreamId));
        public static readonly string ViewName = toSnakeCase(nameof(EvDbViewAddress.ViewName));
        public static readonly string Offset = toSnakeCase(nameof(EvDbStoredSnapshotData.Offset));
        public static readonly string StoreOffset = toSnakeCase(nameof(EvDbStoredSnapshotData.StoreOffset));
        public static readonly string State = toSnakeCase(nameof(EvDbStoredSnapshotData.State));
        public static readonly string Id = toSnakeCase(nameof(EvDbStoredSnapshotData.Id));
    }

    #endregion //  Snapshot
}
