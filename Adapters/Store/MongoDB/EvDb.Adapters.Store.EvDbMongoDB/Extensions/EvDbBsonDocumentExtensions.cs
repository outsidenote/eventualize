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
    #region ToEvent

    public static EvDbEvent ToEvent(this BsonDocument doc)
    {
        var streamType = doc.GetValue(Event.StreamType).AsString;
        var streamId = doc.GetValue(Event.StreamId).AsString;
        var offset = doc.GetValue(Event.Offset).ToInt64();
        var eventType = doc.GetValue(Event.EventType).AsString;
        var capturedBy = doc.GetValue(Event.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Event.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var cursor = new EvDbStreamCursor(streamType, streamId, offset);

        string payloadJson = doc.GetValue(Event.Payload)
                                .AsBsonDocument
                                .ToJson();
        byte[] payload = Encoding.UTF8.GetBytes(payloadJson);

        return new EvDbEvent(eventType, capturedAt, capturedBy, cursor, payload);
    }

    #endregion //  ToEvent

    #region ToMessageRecord

    public static IEvDbMessageMeta ToMessageMeta(this BsonDocument doc)
    {
        EvDbMessageRecord rec = doc.ToMessageRecord();
        IEvDbMessageMeta meta = rec.GetMetadata();
        return meta;
    }

    #endregion //  ToMessageRecord

    #region ToMessageRecord

    public static EvDbMessageRecord ToMessageRecord(this BsonDocument doc)
    {
        var streamType = doc.GetValue(Message.StreamType).AsString;
        var streamId = doc.GetValue(Message.StreamId).AsString;
        var offset = doc.GetValue(Message.Offset).ToInt64();
        var eventType = doc.GetValue(Message.EventType).AsString;
        var capturedBy = doc.GetValue(Message.CapturedBy).AsString;
        var capturedAt = doc.GetValue(Message.CapturedAt).AsBsonDateTime.ToUniversalTime();
        var channel = doc.GetValue(Message.Channel).AsString;
        var serializeType = doc.GetValue(Message.SerializeType).AsString;
        var meaageType = doc.GetValue(Message.MessageType).AsString;

        var otelBson = doc.GetValue(Message.TelemetryContext);
        byte[]? otelContext = null;
        if (otelBson.IsBsonDocument)
        {
            var otlDoc = otelBson.AsBsonDocument;
            string jsonString = otlDoc.ToJson();
            otelContext = Encoding.UTF8.GetBytes(jsonString);
        }

        var payloadDoc = doc.GetValue(Message.Payload).AsBsonDocument;
        var payload = payloadDoc.NormalizePayload(serializeType);

        EvDbTelemetryContextName telemetryContext = otelContext == null
                                    ? EvDbTelemetryContextName.Empty
                                    : EvDbTelemetryContextName.FromArray(otelContext);
        var result = new EvDbMessageRecord
        {
            StreamType = streamType,
            StreamId = streamId,
            Offset = offset,
            EventType = eventType,
            Channel = channel,
            MessageType = meaageType,
            SerializeType = serializeType,
            CapturedAt = capturedAt,
            CapturedBy = capturedBy,
            TelemetryContext = telemetryContext,
            Payload = payload,
        };

        return result;
    }

    #endregion //  ToMessageRecord

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
        var otelContext = activity?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty;
        BsonValue bsonTelemetryContext = otelContext != EvDbTelemetryContextName.Empty
            ? BsonDocument.Parse(Encoding.UTF8.GetString(otelContext))
            : BsonNull.Value;

        return new BsonDocument
        {
            [Event.StreamType] = rec.StreamCursor.StreamType,
            [Event.StreamId] = rec.StreamCursor.StreamId,
            [Event.Offset] = rec.StreamCursor.Offset,
            [Event.EventType] = rec.EventType,
            [Event.TelemetryContext] = bsonTelemetryContext,
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

        byte[]? otelContext = Activity.Current?.SerializeTelemetryContext(); ;
        BsonValue bsonTelemetryContext = otelContext != null
            ? BsonDocument.Parse(Encoding.UTF8.GetString(otelContext))
            : BsonNull.Value;

        var doc = new BsonDocument
        {
            [Message.StreamType] = rec.StreamType,
            [Message.StreamId] = rec.StreamId,
            [Message.Offset] = rec.Offset,
            [Message.EventType] = rec.EventType,
            [Message.MessageType] = rec.MessageType,
            [Message.Channel] = rec.Channel.ToString(),
            [Message.SerializeType] = rec.SerializeType,
            [Message.ShardName] = shardName.ToString(),
            [Message.TelemetryContext] = bsonTelemetryContext,
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

    #region NormilizeTelemetryContext

    /// <summary>
    /// Normalizes the OTEL context.
    /// The payload is a Bson byte[] representation of BsonDocument
    /// </summary>
    /// <param name="bson">The BSON representation.</param>
    /// <returns>Byte[] that can be deserialize using System.Text.Json</returns>
    private static byte[]? NormalizeTelemetryContext(this BsonValue bson)
    {
        if (bson.IsBsonNull)
            return null;
        // Deserialize the BsonValue to a BsonDocument
        var doc = bson.AsBsonDocument;
        // Convert the BsonDocument to a JSON string and then to a byte[]
        // This is assuming that the it is a valid JSON 
        string jsonString = doc.ToJson();
        byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
        return jsonBytes;
    }

    #endregion //  NormalizeTelemetryContext

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
