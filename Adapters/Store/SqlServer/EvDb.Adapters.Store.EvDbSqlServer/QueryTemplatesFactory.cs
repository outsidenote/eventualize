using EvDb.Core;
using EvDb.Core.Adapters;
using System.Text.Json;

namespace EvDb.Adapters.Store.SqlServer;

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
                WHERE {toSnakeCase(nameof(EvDbViewAddress.Domain))} = @{nameof(EvDbViewAddress.Domain)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.Partition))} = @{nameof(EvDbViewAddress.Partition)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.StreamId))} = @{nameof(EvDbViewAddress.StreamId)}
                    AND {toSnakeCase(nameof(EvDbViewAddress.ViewName))} = @{nameof(EvDbViewAddress.ViewName)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))} as {nameof(EvDbEventRecord.Domain)},
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))} as {nameof(EvDbEventRecord.Partition)},
                    {toSnakeCase(nameof(EvDbEventRecord.StreamId))} as {nameof(EvDbEventRecord.StreamId)},
                    {toSnakeCase(nameof(EvDbEventRecord.Offset))} as {nameof(EvDbEventRecord.Offset)},
                    {toSnakeCase(nameof(EvDbEventRecord.EventType))} as {nameof(EvDbEventRecord.EventType)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} as {nameof(EvDbEventRecord.CapturedAt)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} as {nameof(EvDbEventRecord.CapturedBy)},
                    {toSnakeCase(nameof(EvDbEventRecord.Payload))} as {nameof(EvDbEventRecord.Payload)}                  
                FROM {storageContext}event
                WHERE {toSnakeCase(nameof(EvDbStreamCursor.Domain))} = @{nameof(EvDbStreamCursor.Domain)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Partition))} = @{nameof(EvDbStreamCursor.Partition)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.StreamId))} = @{nameof(EvDbStreamCursor.StreamId)}
                    and {toSnakeCase(nameof(EvDbStreamCursor.Offset))} >= @{nameof(EvDbStreamCursor.Offset)};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            SaveEvents = $"""
                    INSERT INTO {storageContext}event (
                        {toSnakeCase(nameof(EvDbEventRecord.Domain))},
                        {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.StreamId))},
                        {toSnakeCase(nameof(EvDbEventRecord.Offset))},
                        {toSnakeCase(nameof(EvDbEventRecord.EventType))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.Payload))},
                        {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))},
                        {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}) 
                    VALUES (
                        @{nameof(EvDbEventRecord.Domain)}, 
                        @{nameof(EvDbEventRecord.Partition)}, 
                        @{nameof(EvDbEventRecord.StreamId)}, 
                        @{nameof(EvDbEventRecord.Offset)}, 
                        @{nameof(EvDbEventRecord.EventType)}, 
                        @{nameof(EvDbEventRecord.Payload)},
                        @{nameof(EvDbEventRecord.CapturedBy)},
                        @{nameof(EvDbEventRecord.CapturedAt)})
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
