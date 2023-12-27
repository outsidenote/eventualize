using Eventualize.Core;

namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const

internal static class QueryTemplatesFactory
{
    public static EventualizeAdapterQueryTemplates Create(EventualizeStorageContext storageContext)
    {
        return new EventualizeAdapterQueryTemplates
        {
            GetLastSnapshotSnapshot = $"""
                SELECT MAX(offset)
                    FROM {storageContext}snapshot
                    WHERE domain = @{nameof(EventualizeStreamUri.Domain)}
                        AND stream_type = @{nameof(EventualizeStreamUri.StreamType)}
                        AND stream_id = @{nameof(EventualizeStreamUri.StreamId)}
                """,
            TryGetSnapshot = $"""
                SELECT json_data as {nameof(EventualizeStoredSnapshot<object>.State)}, offset as {nameof(EventualizeStoredSnapshot<object>.Cursor.Offset)}
                FROM {storageContext}snapshot
                WHERE domain = @{nameof(EventualizeStreamUri.Domain)}
                    AND stream_type = @{nameof(EventualizeStreamUri.StreamType)}
                    AND stream_id = @{nameof(EventualizeStreamUri.StreamId)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    event_type as {nameof(EventualizeStoredEvent.EventType)},
                    captured_at as {nameof(EventualizeStoredEvent.CapturedAt)},
                    captured_by as {nameof(EventualizeStoredEvent.CapturedBy)},
                    json_data as {nameof(EventualizeStoredEvent.JsonData)},
                    stored_at as {nameof(EventualizeStoredEvent.StoredAt)}                    
                FROM {storageContext}event
                WHERE domain = @{nameof(EventualizeStreamCursor.Domain)}
                    AND stream_type = @{nameof(EventualizeStreamCursor.StreamType)}
                    AND stream_id = @{nameof(EventualizeStreamCursor.StreamId)}
                    and offset >= @{nameof(EventualizeStreamCursor.Offset)};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            Save = $"""
                    INSERT INTO {storageContext}event (
                        stream_id,
                        stream_type, 
                        event_type, 
                        offset,
                        json_data,
                        captured_by,
                        captured_at, 
                        domain) 
                    VALUES (
                        @{nameof(AggregateSaveParameter.AggregateId)}, 
                        @{nameof(AggregateSaveParameter.AggregateType)}, 
                        @{nameof(AggregateSaveParameter.EventType)}, 
                        @{nameof(AggregateSaveParameter.Sequence)}, 
                        @{nameof(AggregateSaveParameter.Payload)},
                        @{nameof(AggregateSaveParameter.CapturedBy)},
                        @{nameof(AggregateSaveParameter.CapturedAt)}, 
                        @{nameof(AggregateSaveParameter.Domain)})
                    """,
            SaveSnapshot = $"""
            INSERT INTO {storageContext}snapshot (
                        stream_id,
                        stream_type, 
                        offset,
                        json_data,
                        domain)
            VALUES (
                        @{nameof(SnapshotSaveParameter.StreamId)},
                        @{nameof(SnapshotSaveParameter.StreamType)},
                        @{nameof(SnapshotSaveParameter.Offset)},
                        @{nameof(SnapshotSaveParameter.Payload)},
                        @{nameof(SnapshotSaveParameter.Domain)})
            """

        };
    }
}
