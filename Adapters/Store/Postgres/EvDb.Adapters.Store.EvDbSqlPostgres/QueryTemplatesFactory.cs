using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.Postgres;

// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const

internal static class QueryTemplatesFactory
{
    public static EvDbAdapterQueryTemplates Create(EvDbStorageContext storageContext)
    {
        return new EvDbAdapterQueryTemplates
        {
            GetLastSnapshotSnapshot = $"""
                SELECT MAX(offset)
                    FROM {storageContext}snapshot
                    WHERE domain = @{nameof(EvDbStreamId.Domain)}
                        AND stream_type = @{nameof(EvDbStreamId.EntityType)}
                        AND stream_id = @{nameof(EvDbStreamId.EntityId)}
                """,
            TryGetSnapshot = $"""
                SELECT json_data as {nameof(EvDbStoredSnapshot<object>.State)}, offset as {nameof(EvDbStoredSnapshot<object>.Cursor.Offset)}
                FROM {storageContext}snapshot
                WHERE domain = @{nameof(EvDbStreamId.Domain)}
                    AND stream_type = @{nameof(EvDbStreamId.EntityType)}
                    AND stream_id = @{nameof(EvDbStreamId.EntityId)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    event_type as {nameof(EvDbStoredEvent.EventType)},
                    captured_at as {nameof(EvDbStoredEvent.CapturedAt)},
                    captured_by as {nameof(EvDbStoredEvent.CapturedBy)},
                    json_data as {nameof(EvDbStoredEvent.Data)},
                    stored_at as {nameof(EvDbStoredEvent.StoredAt)}                    
                FROM {storageContext}event
                WHERE domain = @{nameof(EvDbStreamCursor.Domain)}
                    AND stream_type = @{nameof(EvDbStreamCursor.EntityType)}
                    AND stream_id = @{nameof(EvDbStreamCursor.EntityId)}
                    and offset >= @{nameof(EvDbStreamCursor.Offset)};
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
                        @{nameof(SnapshotSaveParameter.EntityId)},
                        @{nameof(SnapshotSaveParameter.EntityType)},
                        @{nameof(SnapshotSaveParameter.Offset)},
                        @{nameof(SnapshotSaveParameter.Payload)},
                        @{nameof(SnapshotSaveParameter.Domain)})
            """

        };
    }
}
