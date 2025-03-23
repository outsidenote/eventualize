using MongoDB.Bson;
using System.Diagnostics;
using System.Text;

namespace MongoBenchmark;

// A simplified version of the provided extension method for converting an event to a BsonDocument.
public static class EvDbBsonDocumentExtensions
{
    public static BsonDocument ToBsonDocument(this EvDbEvent rec)
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
                { "trace_id", traceId != null ? (BsonValue) traceId : BsonNull.Value },
                { "span_id", spanId != null ? (BsonValue) spanId : BsonNull.Value },
                { "payload", payload },
                { "captured_by", rec.CapturedBy },
                { "captured_at", new BsonDateTime(rec.CapturedAt.UtcDateTime) }
            };
    }
}
