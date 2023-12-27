using Eventualize.Core;
using Eventualize.Core.Abstractions;

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
                SELECT
                    json_data as {nameof(EventualizeeSnapshotRelationalRecrod.SerializedState)},
                    domain as {nameof(EventualizeeSnapshotRelationalRecrod.Domain)},
                    stream_type as {nameof(EventualizeeSnapshotRelationalRecrod.StreamType)},
                    stream_id as {nameof(EventualizeeSnapshotRelationalRecrod.StreamId)},
                    aggregate_type as {nameof(EventualizeeSnapshotRelationalRecrod.AggregateType)},
                    offset as {nameof(EventualizeeSnapshotRelationalRecrod.Offset)}
                FROM {storageContext}snapshot
                WHERE domain = @{nameof(EventualizeSnapshotUri.Domain)}
                    AND stream_type = @{nameof(EventualizeSnapshotUri.StreamType)}
                    AND stream_id = @{nameof(EventualizeSnapshotUri.StreamId)}
                    AND aggregate_type = @{nameof(EventualizeSnapshotUri.AggregateType)}
                ORDER BY offset DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    event_type as {nameof(EventualizeStoredEventEntity.EventType)},
                    captured_at as {nameof(EventualizeStoredEventEntity.CapturedAt)},
                    captured_by as {nameof(EventualizeStoredEventEntity.CapturedBy)},
                    json_data as {nameof(EventualizeStoredEventEntity.Data)},
                    stored_at as {nameof(EventualizeStoredEventEntity.StoredAt)},
                    offset as {nameof(EventualizeStoredEventEntity.Offset)},
                    domain  as {nameof(EventualizeStoredEventEntity.Domain)} ,
                    stream_type as {nameof(EventualizeStoredEventEntity.StreamType)},
                    stream_id as  {nameof(EventualizeStoredEventEntity.StreamId)}
                
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
