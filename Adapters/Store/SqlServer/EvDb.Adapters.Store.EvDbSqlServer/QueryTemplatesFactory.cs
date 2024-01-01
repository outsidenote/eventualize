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
                    WHERE domain = @{nameof(EvDbStreamId.Domain)}
                        AND stream_type = @{nameof(EvDbStreamId.EntityType)}
                        AND stream_id = @{nameof(EvDbStreamId.EntityId)}
                """,
            TryGetSnapshot = $"""
                SELECT
                    json_data as {nameof(EvDbeSnapshotRelationalRecrod.SerializedState)},
                    domain as {nameof(EvDbeSnapshotRelationalRecrod.Domain)},
                    stream_type as {nameof(EvDbeSnapshotRelationalRecrod.EntityType)},
                    stream_id as {nameof(EvDbeSnapshotRelationalRecrod.EntityId)},
                    aggregate_type as {nameof(EvDbeSnapshotRelationalRecrod.AggregateType)},
                    offset as {nameof(EvDbeSnapshotRelationalRecrod.Offset)}
                FROM {storageContext}snapshot
                WHERE domain = @{nameof(EvDbSnapshotId.Domain)}
                    AND stream_type = @{nameof(EvDbSnapshotId.EntityType)}
                    AND stream_id = @{nameof(EvDbSnapshotId.EntityId)}
                    AND aggregate_type = @{nameof(EvDbSnapshotId.AggregateType)}
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
                    stream_type as {nameof(EvDbStoredEventEntity.EntityType)},
                    stream_id as  {nameof(EvDbStoredEventEntity.EntityId)}
                
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
                        domain,
                        stream_type, 
                        stream_id,
                        aggregate_type,
                        offset,
                        json_data
                        )
            VALUES (
                        @{nameof(SnapshotSaveParameter.Domain)},
                        @{nameof(SnapshotSaveParameter.EntityType)},
                        @{nameof(SnapshotSaveParameter.EntityId)},
                        @{nameof(SnapshotSaveParameter.AggregateType)},
                        @{nameof(SnapshotSaveParameter.Offset)},
                        @{nameof(SnapshotSaveParameter.Payload)}
                    )
            """

        };
    }
}
