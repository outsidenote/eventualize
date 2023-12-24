namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-19] all parameters and field should be driven from nameof or const

internal static class QueryTemplatesFactory
{
    public static EventualizeAdapterQueryTemplates Create(EventualizeStorageContext storageContext)
    {
        return new EventualizeAdapterQueryTemplates
        {
            GetLastSnapshotSnapshot = $"""
                SELECT MAX(sequence_id)
                    FROM {storageContext}snapshot
                    WHERE domain = 'default'
                        AND aggregate_type = @{nameof(AggregateParameter.Type)}
                        AND aggregate_id = @{nameof(AggregateParameter.Id)}
                """,
            TryGetSnapshot = $"""
                SELECT json_data as {nameof(EventualizeStoredSnapshotData<object>.Snapshot)}, sequence_id as {nameof(EventualizeStoredSnapshotData<object>.SnapshotOffset)}
                FROM {storageContext}snapshot
                WHERE domain = 'default'
                    AND aggregate_type = @{nameof(AggregateParameter.Type)}
                    AND aggregate_id = @{nameof(AggregateParameter.Id)}
                ORDER BY sequence_id DESC
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
                WHERE domain = 'default'
                    AND aggregate_type = @{nameof(AggregateParameter.Type)}
                    AND aggregate_id = @{nameof(AggregateParameter.Id)}
                    and sequence_id >= @{nameof(AggregateSequenceParameter.Sequence)};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            Save = $"""
                    INSERT INTO {storageContext}event (
                        aggregate_id,
                        aggregate_type, 
                        event_type, 
                        sequence_id,
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
                        aggregate_id,
                        aggregate_type, 
                        sequence_id,
                        json_data,
                        domain)
            VALUES (
                        @{nameof(SnapshotSaveParameter.AggregateId)},
                        @{nameof(SnapshotSaveParameter.AggregateType)},
                        @{nameof(SnapshotSaveParameter.Sequence)},
                        @{nameof(SnapshotSaveParameter.Payload)},
                        @{nameof(SnapshotSaveParameter.Domain)})
            """

        };
    }
}
