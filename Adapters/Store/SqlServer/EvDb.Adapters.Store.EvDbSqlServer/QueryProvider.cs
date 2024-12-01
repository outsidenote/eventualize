using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.SqlServer;

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
                    {toSnakeCase(nameof(EvDbEventRecord.Offset))} as {nameof(EvDbEventRecord.Offset)},
                    {toSnakeCase(nameof(EvDbEventRecord.EventType))} as {nameof(EvDbEventRecord.EventType)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} as {nameof(EvDbEventRecord.CapturedAt)},
                    {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} as {nameof(EvDbEventRecord.CapturedBy)},
                    {toSnakeCase(nameof(EvDbEventRecord.Payload))} as {nameof(EvDbEventRecord.Payload)}                  
                FROM {tblInitial}events WITH (READCOMMITTEDLOCK)
                WHERE {toSnakeCase(nameof(EvDbStreamCursor.Domain))} = @{nameof(EvDbStreamCursor.Domain)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Partition))} = @{nameof(EvDbStreamCursor.Partition)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.StreamId))} = @{nameof(EvDbStreamCursor.StreamId)}
                    AND {toSnakeCase(nameof(EvDbStreamCursor.Offset))} >= @{nameof(EvDbStreamCursor.Offset)};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            SaveEvents = $"{tblInitial}InsertEventsBatch_Events",
            SaveToOutbox = $$"""{{tblInitial}}InsertOutboxBatch_{0}"""
        };
    }

    public static EvDbSnapshotAdapterQueryTemplates CreateSnapshotQueries(EvDbStorageContext storageContext)
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;
        string tabInitial = storageContext.Id;

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
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
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
