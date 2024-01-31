using EvDb.Core;
using EvDb.Core.Adapters;
using System.Text.Json;

namespace EvDb.Adapters.Store.Postgres;

// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const

internal static class QueryTemplatesFactory
{
    public static EvDbAdapterQueryTemplates Create(EvDbStorageContext storageContext)
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        return new EvDbAdapterQueryTemplates
        {
            GetSnapshot = $"""
                SELECT {toSnakeCase(nameof(EvDbStoredSnapshot.State))} as {nameof(EvDbStoredSnapshot.State)}, 
                        {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} as {nameof(EvDbStoredSnapshot.Offset)}
                FROM {storageContext}snapshot
                WHERE {toSnakeCase(nameof(EvDbStreamAddress.Domain))} = @{nameof(EvDbStreamAddress.Domain)}
                    AND {toSnakeCase(nameof(EvDbStreamAddress.Partition))} = @{nameof(EvDbStreamAddress.Partition)}
                    AND {toSnakeCase(nameof(EvDbStreamAddress.StreamId))} = @{nameof(EvDbStreamAddress.StreamId)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    {toSnakeCase(nameof(EvDbEvent.StreamCursor.))} as {nameof(EvDbEvent.EventType)},
                    {toSnakeCase(nameof(EvDbEvent.EventType))} as {nameof(EvDbEvent.EventType)},
                    {toSnakeCase(nameof(EvDbEvent.CapturedAt))} as {nameof(EvDbEvent.CapturedAt)},
                    {toSnakeCase(nameof(EvDbEvent.CapturedBy))} as {nameof(EvDbEvent.CapturedBy)},
                    {toSnakeCase(nameof(EvDbEvent.Payload))} as {nameof(EvDbEvent.Payload)}                  
                FROM {storageContext}event
                WHERE {toSnakeCase(nameof(EvDbStreamCursor.Domain))} = @{nameof(EvDbStreamCursor.Domain)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Partition))} = @{nameof(EvDbStreamCursor.Partition)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.StreamId))} = @{nameof(EvDbStreamCursor.StreamId)}
                    and {toSnakeCase(nameof(EvDbStreamCursor.Offset))} >= @{nameof(EvDbStreamCursor.Offset)};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            SaveEvents = $"""
                    INSERT INTO {storageContext}event (
                        {toSnakeCase(nameof(StoreEventsParameter.Domain))},
                        {toSnakeCase(nameof(StoreEventsParameter.Partition))}, 
                        {toSnakeCase(nameof(StoreEventsParameter.StreamId))},
                        {toSnakeCase(nameof(StoreEventsParameter.Offset))},
                        {toSnakeCase(nameof(StoreEventsParameter.EventType))}, 
                        {toSnakeCase(nameof(StoreEventsParameter.Payload))},
                        {toSnakeCase(nameof(StoreEventsParameter.CapturedBy))},
                        {toSnakeCase(nameof(StoreEventsParameter.CapturedAt))}) 
                    VALUES (
                        @{nameof(StoreEventsParameter.Domain)}, 
                        @{nameof(StoreEventsParameter.Partition)}, 
                        @{nameof(StoreEventsParameter.StreamId)}, 
                        @{nameof(StoreEventsParameter.Offset)}, 
                        @{nameof(StoreEventsParameter.EventType)}, 
                        @{nameof(StoreEventsParameter.Payload)},
                        @{nameof(StoreEventsParameter.CapturedBy)},
                        @{nameof(StoreEventsParameter.CapturedAt)})
                    """,
            SaveSnapshot = $"""
            INSERT INTO {storageContext}snapshot (
                        {toSnakeCase(nameof(SnapshotSaveParameter.Domain))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.Partition))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.StreamId))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.ViewName))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.Offset))},
                        {toSnakeCase(nameof(SnapshotSaveParameter.State))})
            VALUES (
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
