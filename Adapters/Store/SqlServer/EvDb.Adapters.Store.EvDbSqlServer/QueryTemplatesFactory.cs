using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.SqlServer;

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
                    WHERE domain = @{nameof(EvDbStreamUri.Domain)}
                        AND stream_type = @{nameof(EvDbStreamUri.StreamType)}
                        AND stream_id = @{nameof(EvDbStreamUri.StreamId)}
                """,
            TryGetSnapshot = $"""
                SELECT
                    json_data as {nameof(EvDbeSnapshotRelationalRecrod.SerializedState)},
                    domain as {nameof(EvDbeSnapshotRelationalRecrod.Domain)},
                    stream_type as {nameof(EvDbeSnapshotRelationalRecrod.StreamType)},
                    stream_id as {nameof(EvDbeSnapshotRelationalRecrod.StreamId)},
                    aggregate_type as {nameof(EvDbeSnapshotRelationalRecrod.AggregateType)},
                    offset as {nameof(EvDbeSnapshotRelationalRecrod.Offset)}
                FROM {storageContext}snapshot
                WHERE domain = @{nameof(EvDbSnapshotUri.Domain)}
                    AND stream_type = @{nameof(EvDbSnapshotUri.StreamType)}
                    AND stream_id = @{nameof(EvDbSnapshotUri.StreamId)}
                    AND aggregate_type = @{nameof(EvDbSnapshotUri.AggregateType)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    event_type as {nameof(EvDbStoredEventEntity.EventType)},
                    captured_at as {nameof(EvDbStoredEventEntity.CapturedAt)},
                    captured_by as {nameof(EvDbStoredEventEntity.CapturedBy)},
                    json_data as {nameof(EvDbStoredEventEntity.Data)},
                    stored_at as {nameof(EvDbStoredEventEntity.StoredAt)},
                    offset as {nameof(EvDbStoredEventEntity.Offset)},
                    domain  as {nameof(EvDbStoredEventEntity.Domain)} ,
                    stream_type as {nameof(EvDbStoredEventEntity.StreamType)},
                    stream_id as  {nameof(EvDbStoredEventEntity.StreamId)}
                
                FROM {storageContext}event
                WHERE domain = @{nameof(EvDbStreamCursor.Domain)}
                    AND stream_type = @{nameof(EvDbStreamCursor.StreamType)}
                    AND stream_id = @{nameof(EvDbStreamCursor.StreamId)}
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
                        domain,
                        stream_type, 
                        stream_id,
                        aggregate_type,
                        offset,
                        json_data
                        )
            VALUES (
                        @{nameof(SnapshotSaveParameter.Domain)},
                        @{nameof(SnapshotSaveParameter.StreamType)},
                        @{nameof(SnapshotSaveParameter.StreamId)},
                        @{nameof(SnapshotSaveParameter.AggregateType)},
                        @{nameof(SnapshotSaveParameter.Offset)},
                        @{nameof(SnapshotSaveParameter.Payload)}
                    )
            """

        };
    }
}
