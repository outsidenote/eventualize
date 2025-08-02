// Ignore Spelling: Bson Calc

using EvDb.Adapters.Store.MongoDB.Internals;
using EvDb.Core;
using EvDb.Core.Adapters;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using System.Diagnostics;
using System.Text;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames.Fields;

namespace EvDb.Adapters.Store.Internals;

public static class EvDbBsonDocumentExtensions
{
    private const string MONGO_DB_ID = "_id";

    #region GetObjectId

    public static ObjectId? GetObjectId(this BsonDocument document)
    {
        if (!document.TryGetValue("_id", out BsonValue value))
            return null;

        return value.BsonType switch
        {
            BsonType.ObjectId => value.AsObjectId,
            BsonType.String when ObjectId.TryParse(value.AsString, out ObjectId parsed) => parsed,
            _ => null
        };
    }

    #endregion //  GetObjectId

    #region ExtractStoredAt

    /// <summary>
    /// Extracts the stored at time-stamp from the BsonDocument ObjectId.
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static DateTimeOffset? ExtractStoredAt(this BsonDocument doc)
    {
        if (doc.TryGetValue(MONGO_DB_ID, out var idValue) && idValue.IsObjectId)
        {
            var objectId = idValue.AsObjectId;
            return new DateTimeOffset(objectId.CreationTime, TimeSpan.Zero); // UTC
        }

        return null; // Not an ObjectId or _id missing
    }

    #endregion //  ExtractStoredAt

    #region ToEvent

    public static EvDbEvent ToEvent(this BsonDocument doc)
    {
        var streamType = doc.GetValue(Event.StreamType).AsString;
        var streamId = doc.GetValue(Event.StreamId).AsString;
        var offset = doc.GetValue(Event.Offset).ToInt64();
        var eventType = doc.GetValue(Event.EventType).AsString;
        var capturedBy = doc.GetValue(Event.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Event.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var storedAt = doc.ExtractStoredAt();
        var cursor = new EvDbStreamCursor(streamType, streamId, offset);

        string payloadJson = doc.GetValue(Event.Payload)
                                .AsBsonDocument
                                .ToJson();
        byte[] payload = Encoding.UTF8.GetBytes(payloadJson);

        return new EvDbEvent(eventType, capturedAt, capturedBy, cursor, payload)
        {
            StoredAt = storedAt,
        };
    }

    #endregion //  ToEvent

    #region ToMessageMeta

    public static IEvDbMessageMeta ToMessageMeta(this BsonDocument doc)
    {
        EvDbMessageRecord rec = doc.ToMessageRecord();
        IEvDbMessageMeta meta = rec.GetMetadata();
        return meta;
    }

    #endregion //  ToMessageMeta

    #region ToMessageRecord

    public static EvDbMessageRecord ToMessageRecord(this BsonDocument doc)
    {
        var id = doc.GetValue(Message.Id).AsGuid;
        var streamType = doc.GetValue(Message.StreamType).AsString;
        var streamId = doc.GetValue(Message.StreamId).AsString;
        var offset = doc.GetValue(Message.Offset).ToInt64();
        var eventType = doc.GetValue(Message.EventType).AsString;
        var capturedBy = doc.GetValue(Message.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Message.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var storedAt = doc.ExtractStoredAt();
        var channel = doc.GetValue(Message.Channel).AsString;
        var serializeType = doc.GetValue(Message.SerializeType).AsString;
        var meaageType = doc.GetValue(Message.MessageType).AsString;

        var payloadDoc = doc.GetValue(Message.Payload).AsBsonDocument;
        var payload = payloadDoc.NormalizePayload(serializeType);

        var traceParentValue = doc.GetValue(Message.TraceParent);
        EvDbOtelTraceParent traceParent = traceParentValue.IsBsonDocument
                                    ? EvDbOtelTraceParent.Empty
                                    : traceParentValue.AsString;
        var result = new EvDbMessageRecord
        {
            Id = id,
            StreamType = streamType,
            StreamId = streamId,
            Offset = offset,
            EventType = eventType,
            Channel = channel,
            MessageType = meaageType,
            SerializeType = serializeType,
            CapturedAt = capturedAt,
            CapturedBy = capturedBy,
            TraceParent = traceParent,
            Payload = payload,
            StoredAt = storedAt
        };

        return result;
    }

    #endregion //  ToMessageRecord

    #region ToMessag

    public static EvDbMessage ToMessage(this BsonDocument doc)
    {
        var id = doc.GetValue(Message.Id).AsGuid;
        var streamType = doc.GetValue(Message.StreamType).AsString;
        var streamId = doc.GetValue(Message.StreamId).AsString;
        var offset = doc.GetValue(Message.Offset).ToInt64();
        var eventType = doc.GetValue(Message.EventType).AsString;
        var capturedBy = doc.GetValue(Message.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Message.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var storedAt = doc.ExtractStoredAt();
        var channel = doc.GetValue(Message.Channel).AsString;
        var serializeType = doc.GetValue(Message.SerializeType).AsString;
        var meaageType = doc.GetValue(Message.MessageType).AsString;

        var otelBson = doc.GetValue(Message.TraceParent);
        byte[]? otelContext = null;
        if (otelBson.IsBsonDocument)
        {
            var otlDoc = otelBson.AsBsonDocument;
            string jsonString = otlDoc.ToJson();
            otelContext = Encoding.UTF8.GetBytes(jsonString);
        }

        var payloadDoc = doc.GetValue(Message.Payload).AsBsonDocument;
        var payload = payloadDoc.NormalizePayload(serializeType);

        EvDbOtelTraceParent telemetryContext = otelContext == null
                                    ? EvDbOtelTraceParent.Empty
                                    : EvDbOtelTraceParent.FromArray(otelContext);
        var cursor = new EvDbStreamCursor(streamType, streamId, offset);
        var result = new EvDbMessage
        {
            Id = id,
            StreamCursor = cursor,
            EventType = eventType,
            Channel = channel,
            MessageType = meaageType,
            SerializeType = serializeType,
            CapturedAt = capturedAt,
            CapturedBy = capturedBy,
            TraceParent = telemetryContext,
            Payload = payload,
            StoredAt = storedAt
        };

        return result;
    }

    #endregion //  ToMessage

    #region ToSnapshotData

    public static EvDbStoredSnapshotData ToSnapshotData(this BsonDocument doc)
    {
        // Map fields from the BsonDocument back to an EvDbEvent.
        var streamType = doc.GetValue(Snapshot.StreamType).AsString;
        var streamId = doc.GetValue(Snapshot.StreamId).AsString;
        var viewName = doc.GetValue(Snapshot.ViewName).AsString;
        var offset = doc.GetValue(Snapshot.Offset).ToInt64();
        var storeOffset = doc.GetValue(Snapshot.StoreOffset).ToInt64();
        var address = new EvDbViewAddress(streamType, streamId, viewName);

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

    public static EvDbStoredSnapshotResult ToSnapshotInfo(this BsonDocument doc)
    {
        var storeOffset = doc.GetValue(Snapshot.Offset).ToInt64();
        DateTimeOffset? storedAt = doc.TryGetValue(Snapshot.StoredAt, out var storedAtValue) && storedAtValue != BsonNull.Value
            ? storedAtValue.ToNullableUniversalTime()
            : null;

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

        return new EvDbStoredSnapshotResult(storeOffset, storedAt, state);
    }

    #endregion //  ToSnapshotInfo

    #region EvDbToBsonDocument(EvDbEventRecord rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbEvent rec)
    {
        string json = Encoding.UTF8.GetString(rec.Payload);
        BsonDocument payload = BsonDocument.Parse(json);

        var activity = Activity.Current;
        var otelContext = activity?.SerializeTelemetryContext() ?? EvDbOtelTraceParent.Empty;
        BsonValue bsonTelemetryContext = otelContext != EvDbOtelTraceParent.Empty && !string.IsNullOrEmpty(otelContext)
            ? BsonString.Create((string?)otelContext)
            : BsonNull.Value;

        return new BsonDocument
        {
            [Event.StreamType] = rec.StreamCursor.StreamType,
            [Event.StreamId] = rec.StreamCursor.StreamId,
            [Event.Offset] = rec.StreamCursor.Offset,
            [Event.EventType] = rec.EventType.Value,
            [Event.TraceParent] = bsonTelemetryContext,
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

        string? otelContext = Activity.Current?.SerializeTelemetryContext();
        BsonValue bsonTelemetryContext = otelContext != null
            ? BsonString.Create(otelContext)
            : BsonNull.Value;

        var doc = new BsonDocument
        {
            //[MONGO_DB_ID] = new BsonBinaryData(rec.Id, GuidRepresentation.Standard),
            [Message.Id] = new BsonBinaryData(rec.Id, GuidRepresentation.Standard),
            [Message.StreamType] = rec.StreamType,
            [Message.StreamId] = rec.StreamId,
            [Message.Offset] = rec.Offset,
            [Message.EventType] = rec.EventType,
            [Message.MessageType] = rec.MessageType,
            [Message.Channel] = rec.Channel.ToString(),
            [Message.SerializeType] = rec.SerializeType,
            [Message.ShardName] = shardName.ToString(),
            [Message.TraceParent] = bsonTelemetryContext,
            [Message.Payload] = payload,
            [Message.CapturedBy] = rec.CapturedBy,
            [Message.CapturedAt] = new BsonDateTime(rec.CapturedAt.UtcDateTime)
        };

        return doc;
    }

    #endregion //  EvDbToBsonDocument(EvDbMessageRecord rec)

    #region EvDbToBsonDocument(EvDbStoredSnapshotData rec)

    public static BsonDocument EvDbToBsonDocument(this EvDbStoredSnapshotData rec)
    {
        BsonDocument doc = new BsonDocument
        {
            [Snapshot.StreamType] = rec.StreamType,
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

    #region CalcCollectionPrefix

    public static string CalcCollectionPrefix(this EvDbStorageContext storageContext)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string collectionPrefix = $"{schema}{storageContext.ShortId}";
        return collectionPrefix;
    }

    #endregion //  CalcCollectionPrefix
}
