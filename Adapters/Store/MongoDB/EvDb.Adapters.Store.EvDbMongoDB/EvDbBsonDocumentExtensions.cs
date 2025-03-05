using EvDb.Core;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using System.Diagnostics;
using System.Text;
using static EvDb.Adapters.Store.EvDbMongoDB.Internals.EvDbFileds;

namespace EvDb.Adapters.Store.Internals;

internal static class EvDbBsonDocumentExtensions
{
    #region ToEvent

    public static EvDbEvent ToEvent(this BsonDocument doc)
    {
        var domain = doc.GetValue(Event.Domain).AsString;
        var partition = doc.GetValue(Event.Partition).AsString;
        var streamId = doc.GetValue(Event.StreamId).AsString;
        var offset = doc.GetValue(Event.Offset).ToInt64();
        var eventType = doc.GetValue(Event.EventType).AsString;
        var payload = doc.GetValue(Event.Payload).AsBsonDocument.ToBson();
        var capturedBy = doc.GetValue(Event.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Event.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var cursor = new EvDbStreamCursor(domain, partition, streamId, offset);
        return new EvDbEvent(eventType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToEvent

    #region ToMessageRecord

    public static EvDbMessage ToMessageRecord(this BsonDocument doc)
    {
        var domain = doc.GetValue(Outbox.Domain).AsString;
        var partition = doc.GetValue(Outbox.Partition).AsString;
        var streamId = doc.GetValue(Outbox.StreamId).AsString;
        var offset = doc.GetValue(Outbox.Offset).ToInt64();
        var eventType = doc.GetValue(Outbox.EventType).AsString;
        var shardName = doc.GetValue(Outbox.ShardName).AsString;
        var payload = doc.GetValue(Outbox.Payload).AsBsonDocument.ToBson();
        var capturedBy = doc.GetValue(Outbox.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Outbox.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var channel = doc.GetValue(Outbox.Channel).AsString;
        var serializeType = doc.GetValue(Outbox.SerializeType).AsString;
        var meaageType = doc.GetValue(Outbox.MessageType).AsString;
        var cursor = new EvDbStreamCursor(domain, partition, streamId, offset);
        return new EvDbMessage(eventType, channel, shardName, meaageType, serializeType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToMessageRecord

    #region ToSnapshotData

    public static EvDbStoredSnapshotData ToSnapshotData(this BsonDocument doc)
    {
        // Map fields from the BsonDocument back to an EvDbEvent.
        var domain = doc.GetValue(Snapshot.Domain).AsString;
        var partition = doc.GetValue(Snapshot.Partition).AsString;
        var streamId = doc.GetValue(Snapshot.StreamId).AsString;
        var viewName = doc.GetValue(Snapshot.ViewName).AsString;
        var offset = doc.GetValue(Snapshot.Offset).ToInt64();
        var storeOffset = doc.GetValue(Snapshot.StoreOffset).ToInt64();
        var state = doc.GetValue(Snapshot.State).AsBsonDocument.ToBson();
        var address = new EvDbViewAddress(domain, partition, streamId, viewName);
        return new EvDbStoredSnapshotData(address, offset, storeOffset, state);
    }

    #endregion //  ToSnapshotData

    #region ToSnapshotInfo

    public static EvDbStoredSnapshot ToSnapshotInfo(this BsonDocument doc)
    {
        var storeOffset = doc.GetValue(Snapshot.StoreOffset).ToInt64();
        var state = doc.GetValue(Snapshot.State).AsBsonDocument.ToBson();
        return new EvDbStoredSnapshot(storeOffset, state);
    }

    #endregion //  ToSnapshotInfo

    #region EvDbToBsonDocument(EvDbEventRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbEvent rec)
    {
        string json = Encoding.UTF8.GetString(rec.Payload);
        var payload = BsonDocument.Parse(json);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        // TODO: [bnaya 2025-02-25] use nameof
        return new BsonDocument
            {
                { Event.Domain, rec.StreamCursor.Domain },
                { Event.Partition, rec.StreamCursor.Partition },
                { Event.StreamId, rec.StreamCursor.StreamId },
                { Event.Offset, rec.StreamCursor.Offset },
                { Event.EventType, rec.EventType },
                { Event.TraceId, traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { Event.SpanId, spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { Event.Payload, payload },
                { Event.CapturedBy, rec.CapturedBy },
                { Event.CapturedAt, new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }

    #endregion //  EvDbToBsonDocument(EvDbEventRecord rec)

    #region EvDbToBsonDocument(EvDbMessageRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbMessage rec)
    {
        BsonDocument payload = GetOutboxPayload(rec.SerializeType, rec.Payload);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        return new BsonDocument
            {
                { Outbox.Domain, rec.StreamCursor.Domain },
                { Outbox.Payload, rec.StreamCursor.Partition },
                { Outbox.StreamId, rec.StreamCursor.StreamId },
                { Outbox.Offset, rec.StreamCursor.Offset },
                { Outbox.EventType, rec.EventType },
                { Outbox.MessageType, rec.MessageType },
                { Outbox.Channel, rec.Channel.ToString() },
                { Outbox.SerializeType, rec.SerializeType },
                { Outbox.ShardName, rec.ShardName.ToString() },
                { Outbox.TraceId,traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { Outbox.SpanId, spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { Outbox.Payload, payload },
                { Outbox.CapturedBy, rec.CapturedBy },
                { Outbox.CapturedAt, new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }

    #endregion //  EvDbToBsonDocument(EvDbMessageRecord rec)

    #region EvDbToBsonDocument(EvDbMessageRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbMessageRecord rec, EvDbShardName shardName)
    {
        BsonDocument payload = GetOutboxPayload(rec.SerializeType, rec.Payload);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        return new BsonDocument
            {
                { Outbox.Domain, rec.Domain },
                { Outbox.Partition, rec.Partition },
                { Outbox.StreamId, rec.StreamId },
                { Outbox.Offset, rec.Offset },
                { Outbox.EventType, rec.EventType },
                { Outbox.MessageType, rec.MessageType },
                { Outbox.Channel, rec.Channel.ToString() },
                { Outbox.SerializeType, rec.SerializeType },
                { Outbox.ShardName, shardName.ToString() },
                { Outbox.TraceId,traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { Outbox.SpanId, spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { Outbox.Payload, payload },
                { Outbox.CapturedBy, rec.CapturedBy },
                { Outbox.CapturedAt, new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }

    #endregion //  EvDbToBsonDocument(EvDbMessageRecord rec)

    #region GetOutboxPayload

    private static BsonDocument GetOutboxPayload(string serializeType, byte[] payload)
    {
        BsonDocument result;
        if (serializeType == IEvDbOutboxSerializer.DefaultFormat)
        {
            string json = Encoding.UTF8.GetString(payload);
            result = BsonDocument.Parse(json);
        }
        else
        {
            result = new BsonDocument
            {
                { "type", serializeType },
                { "payload", new BsonBinaryData(payload) }
            };
        }

        return result;
    }

    #endregion //  GetOutboxPayload

    #region EvDbToBsonDocument(EvDbStoredSnapshotData rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbStoredSnapshotData rec)
    {
        string json = Encoding.UTF8.GetString(rec.State);
        var state = BsonDocument.Parse(json);

        return new BsonDocument
            {
                { Snapshot.Id, new BsonBinaryData(rec.Id, GuidRepresentation.Standard) },
                { Snapshot.Domain, rec.Domain },
                { Snapshot.Partition, rec.Partition },
                { Snapshot.StreamId, rec.StreamId },
                { Snapshot.ViewName, rec.ViewName },
                { Snapshot.Offset, rec.Offset },
                { Snapshot.StoreOffset, rec.StoreOffset },
                { Snapshot.State, state }
            };
    }

    #endregion //  EvDbToBsonDocument(EvDbStoredSnapshotData rec)

    #region CalcCollectionPrefix

    public static string CalcCollectionPrefix(this EvDbStorageContext storageContext)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";
        return tblInitial;
    }

    #endregion //  CalcCollectionPrefix
}
