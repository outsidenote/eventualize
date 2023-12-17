namespace Eventualize.Core.Adapters.SqlStore;

internal static class QueryTemplatesFactory
{
    public static EventualizeAdapterQueryTemplates Create(EventualizeStorageContext storageContext)
    {
        return new EventualizeAdapterQueryTemplates
        {
            GetLastSnapshotSequenceId = $"""
                SELECT MAX(sequence_id)
                    FROM {storageContext}snapshot
                    WHERE domain = 'default'
                        AND aggregate_type = @{EventualizeAdapterParametersConstants.type}
                        AND aggregate_id = @{EventualizeAdapterParametersConstants.id}
                """,
            TryGetSnapshot = $"""
                SELECT json_data, sequence_id
                FROM {storageContext}snapshot
                WHERE domain = 'default'
                    AND aggregate_type = @{EventualizeAdapterParametersConstants.type}
                    AND aggregate_id = @{EventualizeAdapterParametersConstants.id}
                ORDER BY sequence_id DESC
                OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
                """,
            GetEvents = $"""
                SELECT
                    event_type,
                    captured_at,
                    captured_by,
                    json_data,
                    stored_at
                FROM {storageContext}event
                WHERE domain = 'default'
                    AND aggregate_type = @{EventualizeAdapterParametersConstants.type}
                    AND aggregate_id = @{EventualizeAdapterParametersConstants.aggregate_id}
                    and sequence_id >= @{EventualizeAdapterParametersConstants.sequence_id};
                """,
            // take a look at https://www.learndapper.com/saving-data/insert
            Save = $"""
                    INSERT INTO {storageContext}event (
                        domain, 
                        aggregate_type, 
                        aggregate_id,
                        sequence_id,
                        captured_at, 
                        event_type, 
                        captured_by,
                        json_data)
                    VALUES (
                        @domain, 
                        @aggregate_type, 
                        @aggregate_id, 
                        @sequence_id, 
                        @captured_at, 
                        @event_type, 
                        @captured_by,
                        @json_data)
                    """

        };
    }
}
