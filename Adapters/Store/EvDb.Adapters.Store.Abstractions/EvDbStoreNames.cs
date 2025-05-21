namespace EvDb.Core.Adapters.Internals;

public static class EvDbStoreNames
{
    public static class Fields
    {
        private static readonly Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        #region Event

        public static class Event
        {
            public static readonly string Id = toSnakeCase(Projection.Event.Id);
            public static readonly string Domain = toSnakeCase(Projection.Event.Domain);
            public static readonly string Partition = toSnakeCase(Projection.Event.Partition);
            public static readonly string StreamId = toSnakeCase(Projection.Event.StreamId);
            public static readonly string Offset = toSnakeCase(Projection.Event.Offset);
            public static readonly string EventType = toSnakeCase(Projection.Event.EventType);
            public static readonly string Payload = toSnakeCase(Projection.Event.Payload);
            public static readonly string CapturedAt = toSnakeCase(Projection.Event.CapturedAt);
            public static readonly string CapturedBy = toSnakeCase(Projection.Event.CapturedBy);
            public static readonly string TelemetryContext = toSnakeCase(Projection.Event.TelemetryContext);
        }

        #endregion //  Event

        #region Message

        public static class Message
        {
            public static readonly string Id = toSnakeCase(Projection.Message.Id);
            public static readonly string Domain = toSnakeCase(Projection.Message.Domain);
            public static readonly string Partition = toSnakeCase(Projection.Message.Partition);
            public static readonly string StreamId = toSnakeCase(Projection.Message.StreamId);
            public static readonly string Offset = toSnakeCase(Projection.Message.Offset);
            public static readonly string Channel = toSnakeCase(Projection.Message.Channel);
            public static readonly string MessageType = toSnakeCase(Projection.Message.MessageType);
            public static readonly string EventType = toSnakeCase(Projection.Message.EventType);
            public static readonly string ShardName = toSnakeCase(nameof(EvDbMessage.ShardName));
            public static readonly string Payload = toSnakeCase(Projection.Message.Payload);
            public static readonly string CapturedAt = toSnakeCase(Projection.Message.CapturedAt);
            public static readonly string CapturedBy = toSnakeCase(Projection.Message.CapturedBy);
            public static readonly string SerializeType = toSnakeCase(Projection.Message.SerializeType);
            public static readonly string TelemetryContext = toSnakeCase(Projection.Message.TelemetryContext);
        }

        #endregion //  Message

        #region Snapshot

        public static class Snapshot
        {
            public static readonly string Domain = toSnakeCase(Projection.Snapshot.Domain);
            public static readonly string Partition = toSnakeCase(Projection.Snapshot.Partition);
            public static readonly string StreamId = toSnakeCase(Projection.Snapshot.StreamId);
            public static readonly string ViewName = toSnakeCase(Projection.Snapshot.ViewName);
            public static readonly string Offset = toSnakeCase(Projection.Snapshot.Offset);
            public static readonly string StoreOffset = toSnakeCase(Projection.Snapshot.StoreOffset);
            public static readonly string State = toSnakeCase(Projection.Snapshot.State);
            public static readonly string Id = toSnakeCase(Projection.Snapshot.Id);
        }

        #endregion //  Snapshot
    }

    public static class Parameters
    {
        private static readonly Func<string, string> toParamName = name => $"@{name}";

        #region Event

        public static class Event
        {
            public static readonly string Id = toParamName(Projection.Event.Id);
            public static readonly string Domain = toParamName(Projection.Event.Domain);
            public static readonly string Partition = toParamName(Projection.Event.Partition);
            public static readonly string StreamId = toParamName(Projection.Event.StreamId);
            public static readonly string Offset = toParamName(Projection.Event.Offset);
            public static readonly string EventType = toParamName(Projection.Event.EventType);
            public static readonly string Payload = toParamName(Projection.Event.Payload);
            public static readonly string CapturedAt = toParamName(Projection.Event.CapturedAt);
            public static readonly string CapturedBy = toParamName(Projection.Event.CapturedBy);
            public static readonly string TelemetryContext = toParamName(Projection.Event.TelemetryContext);
        }

        #endregion //  Event

        #region Message

        public static class Message
        {
            public static readonly string Id = toParamName(Projection.Message.Id);
            public static readonly string Domain = toParamName(Projection.Message.Domain);
            public static readonly string Partition = toParamName(Projection.Message.Partition);
            public static readonly string StreamId = toParamName(Projection.Message.StreamId);
            public static readonly string Offset = toParamName(Projection.Message.Offset);
            public static readonly string Channel = toParamName(Projection.Message.Channel);
            public static readonly string MessageType = toParamName(Projection.Message.MessageType);
            public static readonly string EventType = toParamName(Projection.Message.EventType);
            public static readonly string ShardName = toParamName(nameof(EvDbMessage.ShardName));
            public static readonly string Payload = toParamName(Projection.Message.Payload);
            public static readonly string CapturedAt = toParamName(Projection.Message.CapturedAt);
            public static readonly string CapturedBy = toParamName(Projection.Message.CapturedBy);
            public static readonly string SerializeType = toParamName(Projection.Message.SerializeType);
            public static readonly string TelemetryContext = toParamName(Projection.Message.TelemetryContext);
        }

        #endregion //  Message

        #region Snapshot

        public static class Snapshot
        {
            public static readonly string Id = toParamName(Projection.Snapshot.Id);
            public static readonly string Domain = toParamName(Projection.Snapshot.Domain);
            public static readonly string Partition = toParamName(Projection.Snapshot.Partition);
            public static readonly string StreamId = toParamName(Projection.Snapshot.StreamId);
            public static readonly string ViewName = toParamName(Projection.Snapshot.ViewName);
            public static readonly string Offset = toParamName(Projection.Snapshot.Offset);
            public static readonly string StoreOffset = toParamName(Projection.Snapshot.StoreOffset);
            public static readonly string State = toParamName(Projection.Snapshot.State);
        }

        #endregion //  Snapshot
    }

    public static class Projection
    {
        #region Event

        public static class Event
        {
            public static readonly string Id = nameof(EvDbEventRecord.Id);
            public static readonly string Domain = nameof(EvDbEventRecord.Domain);
            public static readonly string Partition = nameof(EvDbEventRecord.Partition);
            public static readonly string StreamId = nameof(EvDbEventRecord.StreamId);
            public static readonly string Offset = nameof(EvDbEventRecord.Offset);
            public static readonly string EventType = nameof(EvDbEventRecord.EventType);
            public static readonly string Payload = nameof(EvDbEventRecord.Payload);
            public static readonly string CapturedAt = nameof(EvDbEventRecord.CapturedAt);
            public static readonly string CapturedBy = nameof(EvDbEventRecord.CapturedBy);
            public static readonly string TelemetryContext = nameof(EvDbEventRecord.TelemetryContext);
        }

        #endregion //  Event

        #region Message

        public static class Message
        {
            public static readonly string Id = nameof(EvDbMessageRecord.Id);
            public static readonly string Domain = nameof(EvDbMessageRecord.Domain);
            public static readonly string Partition = nameof(EvDbMessageRecord.Partition);
            public static readonly string StreamId = nameof(EvDbMessageRecord.StreamId);
            public static readonly string Offset = nameof(EvDbMessageRecord.Offset);
            public static readonly string Channel = nameof(EvDbMessageRecord.Channel);
            public static readonly string MessageType = nameof(EvDbMessageRecord.MessageType);
            public static readonly string EventType = nameof(EvDbMessageRecord.EventType);
            public static readonly string ShardName = nameof(EvDbMessage.ShardName);
            public static readonly string Payload = nameof(EvDbMessageRecord.Payload);
            public static readonly string CapturedAt = nameof(EvDbMessageRecord.CapturedAt);
            public static readonly string CapturedBy = nameof(EvDbMessageRecord.CapturedBy);
            public static readonly string SerializeType = nameof(EvDbMessageRecord.SerializeType);
            public static readonly string TelemetryContext = nameof(EvDbEventRecord.TelemetryContext);
        }

        #endregion //  Message

        #region Snapshot

        public static class Snapshot
        {
            public static readonly string Id = nameof(EvDbStoredSnapshotData.Id);
            public static readonly string Domain = nameof(EvDbViewAddress.Domain);
            public static readonly string Partition = nameof(EvDbViewAddress.Partition);
            public static readonly string StreamId = nameof(EvDbViewAddress.StreamId);
            public static readonly string ViewName = nameof(EvDbViewAddress.ViewName);
            public static readonly string Offset = nameof(EvDbStoredSnapshotData.Offset);
            public static readonly string StoreOffset = nameof(EvDbStoredSnapshotData.StoreOffset);
            public static readonly string State = nameof(EvDbStoredSnapshotData.State);
        }

        #endregion //  Snapshot
    }

}