// Ignore Spelling: Bson Calc

using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Diagnostics;
using System.Text;
using static EvDb.Adapters.Store.MongoDB.Internals.EvDbFileds;

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
        var capturedBy = doc.GetValue(Event.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Event.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var cursor = new EvDbStreamCursor(domain, partition, streamId, offset);

        string payloadJson = doc.GetValue(Event.Payload)
                                .AsBsonDocument
                                .ToJson();
        byte[] payload = Encoding.UTF8.GetBytes(payloadJson);

        return new EvDbEvent(eventType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToEvent

    #region ToMessageRecord

    public static EvDbMessageRecord ToMessageRecord(this BsonDocument doc)
    {
        var domain = doc.GetValue(Outbox.Domain).AsString;
        var partition = doc.GetValue(Outbox.Partition).AsString;
        var streamId = doc.GetValue(Outbox.StreamId).AsString;
        var offset = doc.GetValue(Outbox.Offset).ToInt64();
        var eventType = doc.GetValue(Outbox.EventType).AsString;
        var capturedBy = doc.GetValue(Outbox.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Outbox.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var channel = doc.GetValue(Outbox.Channel).AsString;
        var serializeType = doc.GetValue(Outbox.SerializeType).AsString;
        var meaageType = doc.GetValue(Outbox.MessageType).AsString;
        string? traceId = doc.TryGetValue(Outbox.TraceId, out var traceBson) && !traceBson.IsBsonNull
                                ? traceBson.AsString
                                : null;
        string? spanId = doc.TryGetValue(Outbox.SpanId, out var spanBson) && !spanBson.IsBsonNull
                                ? spanBson.AsString
                                : null;

        var payloadDoc = doc.GetValue(Outbox.Payload).AsBsonDocument;
        var payload = payloadDoc.NormalizePayload(serializeType);

        var result = new EvDbMessageRecord
        {
            Domain = domain,
            Partition = partition,
            StreamId = streamId,
            Offset = offset,
            EventType = eventType,
            Channel = channel,
            MessageType = meaageType,
            SerializeType = serializeType,
            CapturedAt = capturedAt,
            CapturedBy = capturedBy,
            Payload = payload,
            TraceId = traceId,
            SpanId = spanId,
        };

        return result;
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
        var address = new EvDbViewAddress(domain, partition, streamId, viewName);

        string stateJson = "null";
        if (doc.TryGetValue(Snapshot.State, out BsonValue value))
        {
            stateJson = value
                            .AsBsonDocument
                            .ToJson();
        }
        byte[] state = Encoding.UTF8.GetBytes(stateJson);

        return new EvDbStoredSnapshotData(address, offset, storeOffset, state);
    }

    #endregion //  ToSnapshotData

    #region ToSnapshotInfo

    public static EvDbStoredSnapshot ToSnapshotInfo(this BsonDocument doc)
    {
        var storeOffset = doc.GetValue(Snapshot.Offset).ToInt64();

        string json = "null";
        if (doc.TryGetValue(Snapshot.State, out BsonValue value))
        {
            if (value is BsonDocument)
            {
                json = value
                            .AsBsonDocument
                            .ToJson();
            }
            else
            {
                json = value.ToJson();
            }
        }

        byte[] state = Encoding.UTF8.GetBytes(json);

        return new EvDbStoredSnapshot(storeOffset, state);
    }

    #endregion //  ToSnapshotInfo

    #region EvDbToBsonDocument(EvDbEventRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbEvent rec)
    {
        string json = Encoding.UTF8.GetString(rec.Payload);
        BsonDocument payload = BsonDocument.Parse(json);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        // TODO: [bnaya 2025-02-25] use nameof
        return new BsonDocument
            {
                [Event.Domain] = rec.StreamCursor.Domain,
                [Event.Partition] = rec.StreamCursor.Partition,
                [Event.StreamId] = rec.StreamCursor.StreamId,
                [Event.Offset] = rec.StreamCursor.Offset,
                [Event.EventType] = rec.EventType,
                [Event.TraceId] = traceId != null ? (BsonValue) traceId : BsonNull.Value,
                [Event.SpanId] = spanId != null ? (BsonValue) spanId : BsonNull.Value,
                [Event.Payload] = payload,
                [Event.CapturedBy] = rec.CapturedBy,
                [Event.CapturedAt] = new BsonDateTime(rec.CapturedAt.UtcDateTime)
            };
    }

    #endregion //  EvDbToBsonDocument(EvDbEventRecord rec)

    #region EvDbToBsonDocument(EvDbMessageRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbMessageRecord rec, EvDbShardName shardName)
    {
        BsonDocument payload = GetOutboxPayload(rec.SerializeType, rec.Payload);

        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToHexString();
        var spanId = activity?.SpanId.ToHexString();

        var doc = new BsonDocument
            {
                [Outbox.Domain] = rec.Domain,
                [Outbox.Partition] = rec.Partition,
                [Outbox.StreamId] = rec.StreamId,
                [Outbox.Offset] = rec.Offset,
                [Outbox.EventType] = rec.EventType,
                [Outbox.MessageType] = rec.MessageType,
                [Outbox.Channel] = rec.Channel.ToString(),
                [Outbox.SerializeType] = rec.SerializeType,
                [Outbox.ShardName] = shardName.ToString(),
                [Outbox.TraceId] =traceId != null ? (BsonValue) traceId : BsonNull.Value,
                [Outbox.SpanId] = spanId != null ? (BsonValue) spanId : BsonNull.Value,
                [Outbox.Payload] = payload,
                [Outbox.CapturedBy] = rec.CapturedBy,
                [Outbox.CapturedAt] = new BsonDateTime(rec.CapturedAt.UtcDateTime)
            };

        return doc;
    }

    #endregion //  EvDbToBsonDocument(EvDbMessageRecord rec)

    #region NormilizePayload

    /// <summary>
    /// Normalizes the payload.
    /// The payload is a Bson byte[] representation of BsonDocument
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="serializaionType">Type of the serializaion.</param>
    /// <returns>Byte[] that can be deserialize using System.Text.Json</returns>
    private static byte[] NormalizePayload(this BsonDocument payload, string serializaionType)
    {
        if (serializaionType != IEvDbOutboxSerializer.DefaultFormat)
        {
            BsonPayload obj = BsonSerializer.Deserialize<BsonPayload>(payload);
            return obj.Payload;
        }

        string jsonString = payload.ToJson();
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
        return jsonBytes;
    }


    #endregion //  NormalizePayload

    #region GetOutboxPayload

    private static BsonDocument GetOutboxPayload(string serializeType, byte[] payload)
    {
        BsonDocument doc;
        if (serializeType == IEvDbOutboxSerializer.DefaultFormat)
        {
            string json = Encoding.UTF8.GetString(payload);
            doc = BsonDocument.Parse(json);
        }
        else
        {
            var bson = new BsonPayload(serializeType, payload);
            doc = bson.ToBsonDocument();
        }

        return doc;
    }

    #endregion //  GetOutboxPayload

    #region EvDbToBsonDocument(EvDbStoredSnapshotData rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbStoredSnapshotData rec)
    {
        BsonDocument doc = new BsonDocument
        {
            [Snapshot.Domain] = rec.Domain,
            [Snapshot.Partition] = rec.Partition,
            [Snapshot.StreamId] = rec.StreamId,
            [Snapshot.ViewName] = rec.ViewName,
            [Snapshot.Offset] = rec.Offset
        };
        if (rec.State.Length > 0)
        {
            string json = Encoding.UTF8.GetString(rec.State);

            if (json != "null")
            {
                BsonValue state = BsonSerializer.Deserialize<BsonValue>(json);
                doc.Add(Snapshot.State, state);
            }
        }
        return doc;
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
