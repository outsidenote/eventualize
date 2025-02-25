using EvDb.Core;
using MongoDB.Bson;
using System.Diagnostics;
using System.Text;

namespace EvDb.Adapters.Store;

internal static class EvDbBsonDocumentExtensions
{
    #region ToEvent

    public static EvDbEvent ToEvent(this BsonDocument doc)
    {
        var domain = doc.GetValue("domain").AsString;
        var partition = doc.GetValue("partition").AsString;
        var streamId = doc.GetValue("stream_id").AsString;
        var offset = doc.GetValue("offset").ToInt64();
        var eventType = doc.GetValue("event_type").AsString;
        var payload = doc.GetValue("state").AsBsonDocument.ToBson();
        var capturedBy = doc.GetValue("captured_by").AsString;
        var capturedAt = doc.GetValue("captured_by").AsBsonDateTime.ToUniversalTime();
        var cursor = new EvDbStreamCursor(domain, partition, streamId, offset);
        return new EvDbEvent(eventType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToEvent

    #region ToMessageRecord

    public static EvDbMessage ToMessageRecord(this BsonDocument doc)
    {
        var domain = doc.GetValue("domain").AsString;
        var partition = doc.GetValue("partition").AsString;
        var streamId = doc.GetValue("stream_id").AsString;
        var offset = doc.GetValue("offset").ToInt64();
        var eventType = doc.GetValue("event_type").AsString;
        var shardName = doc.GetValue("shard-name").AsString;
        var payload = doc.GetValue("payload").AsBsonDocument.ToBson();
        var capturedBy = doc.GetValue("captured_by").AsString;
        var capturedAt = doc.GetValue("captured_by").AsBsonDateTime.ToUniversalTime();
        var channel = doc.GetValue("channel").AsString;
        var serializeType = doc.GetValue("serialize_type").AsString;
        var meaageType = doc.GetValue("message_type").AsString;
        var cursor = new EvDbStreamCursor(domain, partition, streamId, offset);
        return new EvDbMessage(eventType, channel, shardName, meaageType, serializeType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToMessageRecord

    #region ToSnapshotData

    public static EvDbStoredSnapshotData ToSnapshotData(this BsonDocument doc)
    {
        // Map fields from the BsonDocument back to an EvDbEvent.
        var domain = doc.GetValue("domain").AsString;
        var partition = doc.GetValue("partition").AsString;
        var streamId = doc.GetValue("stream_id").AsString;
        var viewName = doc.GetValue("view-name").AsString;
        var offset = doc.GetValue("offset").ToInt64();
        var storeOffset = doc.GetValue("store-offset").ToInt64();
        var state = doc.GetValue("state").AsBsonDocument.ToBson();
        var address = new EvDbViewAddress(domain, partition, streamId, viewName);
        return new EvDbStoredSnapshotData(address, offset, storeOffset, state);
    }

    #endregion //  ToSnapshotData

    #region ToSnapshotInfo

    public static EvDbStoredSnapshot ToSnapshotInfo(this BsonDocument doc)
    {
        var storeOffset = doc.GetValue("store-offset").ToInt64();
        var state = doc.GetValue("state").AsBsonDocument.ToBson();
        return new EvDbStoredSnapshot(storeOffset, state);
    }

    #endregion //  ToSnapshotInfo

    #region ToBsonDocument(EvDbEventRecord rec)

    public static BsonDocument ToBsonDocument(this EvDbEvent rec)
    {
        string json = Encoding.UTF8.GetString(rec.Payload);
        var payload = BsonDocument.Parse(json);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        // TODO: [bnaya 2025-02-25] use nameof
        return new BsonDocument
            {
                { "domain", rec.StreamCursor.Domain },
                { "partition", rec.StreamCursor.Partition },
                { "stream_id", rec.StreamCursor.StreamId },
                { "offset", rec.StreamCursor.Offset },
                { "event_type", rec.EventType },
                { "trace_id", traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { "span_id", spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { "payload", payload },
                { "captured_by", rec.CapturedBy },
                { "captured_at", new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }

    #endregion //  ToBsonDocument(EvDbEventRecord rec)

    #region ToBsonDocument(EvDbMessageRecord rec)

    public static BsonDocument ToBsonDocument(this EvDbMessage rec)
    {
        string json = Encoding.UTF8.GetString(rec.Payload);
        var payload = BsonDocument.Parse(json);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        return new BsonDocument
            {
                { "domain", rec.StreamCursor.Domain },
                { "partition", rec.StreamCursor.Partition },
                { "stream_id", rec.StreamCursor.StreamId },
                { "offset", rec.StreamCursor.Offset },
                { "event_type", rec.EventType },
                { "message_type", rec.MessageType },
                { "channel", rec.Channel.ToString() },
                { "serialize_type", rec.SerializeType },
                { "shard-name", rec.ShardName.ToString() },
                { "trace_id",traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { "span_id", spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { "payload", payload },
                { "captured_by", rec.CapturedBy },
                { "captured_at", new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }

    #endregion //  ToBsonDocument(EvDbMessageRecord rec)

    #region ToBsonDocument(EvDbStoredSnapshotData rec)

    public static BsonDocument ToBsonDocument(this EvDbStoredSnapshotData rec)
    {
        string json = Encoding.UTF8.GetString(rec.State);
        var state = BsonDocument.Parse(json);

        return new BsonDocument
            {
                { "id", new BsonBinaryData(rec.Id, GuidRepresentation.Standard) },
                { "domain", rec.Domain },
                { "partition", rec.Partition },
                { "stream_id", rec.StreamId },
                { "view-name", rec.ViewName },
                { "offset", rec.Offset },
                { "store-offset", rec.StoreOffset },
                { "state", state }
            };
    }

    #endregion //  ToBsonDocument(EvDbStoredSnapshotData rec)
}
