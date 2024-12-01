using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.Postgres;

internal static class QueryProvider
{
    public static EvDbStreamAdapterQueryTemplates CreateStreamQueries(EvDbStorageContext storageContext)
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";

        return new EvDbStreamAdapterQueryTemplates
        {
            GetEvents = $"""
                SELECT
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))} as {nameof(EvDbEventRecord.Domain)},
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))} as {nameof(EvDbEventRecord.Partition)},
                    {toSnakeCase(nameof(EvDbEventRecord.StreamId))} as {nameof(EvDbEventRecord.StreamId)},
                    "{toSnakeCase(nameof(EvDbEventRecord.Offset))}" as {nameof(EvDbEventRecord.Offset)},
                    {toSnakeCase(nameof(EvDbEventRecord.EventType))} as {nameof(EvDbEventRecord.EventType)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} as {nameof(EvDbEventRecord.CapturedAt)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} as {nameof(EvDbEventRecord.CapturedBy)},
                    {toSnakeCase(nameof(EvDbEventRecord.Payload))} as {nameof(EvDbEventRecord.Payload)}                  
                FROM {tblInitial}events
                WHERE {toSnakeCase(nameof(EvDbStreamCursor.Domain))} = @{nameof(EvDbStreamCursor.Domain)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Partition))} = @{nameof(EvDbStreamCursor.Partition)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.StreamId))} = @{nameof(EvDbStreamCursor.StreamId)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Offset))} >= @{nameof(EvDbStreamCursor.Offset)};
                """,
            SaveEvents = $$"""
             INSERT INTO {{tblInitial}}events 
                    ({{toSnakeCase(nameof(EvDbEventRecord.Id))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.Domain))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.Partition))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.StreamId))}}, 
                    "{{toSnakeCase(nameof(EvDbEventRecord.Offset))}}", 
                    {{toSnakeCase(nameof(EvDbEventRecord.EventType))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.TraceId))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.SpanId))}}, 
                    {{toSnakeCase(nameof(EvDbEventRecord.Payload))}})
                SELECT 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.Id))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.Domain))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.Partition))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.StreamId))}}), 
                    UNNEST("{{toSnakeCase(nameof(EvDbEventRecord.Offset))}}"), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.EventType))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.TraceId))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.SpanId))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbEventRecord.Payload))}})
            """,
            SaveToOutbox = $$"""
             INSERT INTO {{tblInitial}}{0} 
                    ({{toSnakeCase(nameof(EvDbMessageRecord.Id))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.Domain))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.Partition))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.StreamId))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.Offset))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.Channel))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.SerializeType))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.EventType))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.TraceId))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.SpanId))}}, 
                    {{toSnakeCase(nameof(EvDbMessageRecord.Payload))}})
                SELECT 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Id))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Domain))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Partition))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.StreamId))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Offset))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Channel))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.SerializeType))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.EventType))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.TraceId))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.SpanId))}}), 
                    UNNEST({{toSnakeCase(nameof(EvDbMessageRecord.Payload))}})
            """
        };
    }

    public static EvDbSnapshotAdapterQueryTemplates CreateSnapshotQueries(EvDbStorageContext storageContext)
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;
        string tabInitial = $"{storageContext.Schema}.{storageContext.Id}";

        return new EvDbSnapshotAdapterQueryTemplates
        {
            GetSnapshot = $"""
                SELECT {toSnakeCase(nameof(EvDbStoredSnapshot.State))} as {nameof(EvDbStoredSnapshot.State)}, 
                        {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} as {nameof(EvDbStoredSnapshot.Offset)}
                FROM {tabInitial}snapshot
                WHERE {toSnakeCase(nameof(EvDbViewAddress.Domain))} = @{nameof(EvDbViewAddress.Domain)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.Partition))} = @{nameof(EvDbViewAddress.Partition)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.StreamId))} = @{nameof(EvDbViewAddress.StreamId)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.ViewName))} = @{nameof(EvDbViewAddress.ViewName)}
                ORDER BY {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} DESC
                LIMIT 1;
                """,
            SaveSnapshot = $"""
            INSERT INTO {tabInitial}snapshot (
                        {toSnakeCase(nameof(SnapshotSaveParameter.Id))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.Domain))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.Partition))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.StreamId))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.ViewName))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.Offset))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.State))})
            VALUES (
                        @{nameof(SnapshotSaveParameter.Id)},
                        @{nameof(SnapshotSaveParameter.Domain)},
                        @{nameof(SnapshotSaveParameter.Partition)},
                        @{nameof(SnapshotSaveParameter.StreamId)},
                        @{nameof(SnapshotSaveParameter.ViewName)},
                        @{nameof(SnapshotSaveParameter.Offset)},
                        @{nameof(SnapshotSaveParameter.State)})
            """
        };
    }
}
