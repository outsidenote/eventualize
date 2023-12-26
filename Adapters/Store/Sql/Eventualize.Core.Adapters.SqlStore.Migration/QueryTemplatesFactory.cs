namespace Eventualize.Core.Adapters.SqlStore;

internal static class QueryTemplatesFactory
{
    public static EventualizeMigrationQueryTemplates Create(EventualizeStorageContext storageContext)
    {
        return new EventualizeMigrationQueryTemplates
        {
            DestroyEnvironment = $"""
            DROP TABLE {storageContext}event;
            DROP TABLE {storageContext}snapshot;            
            """,
            CreateEnvironment = $"""
            -- Create the event table
            CREATE TABLE {storageContext}event (
                domain NVARCHAR(40) NOT NULL,
                stream_type NVARCHAR(40) NOT NULL,
                stream_id NVARCHAR(40) NOT NULL,
                offset BIGINT NOT NULL,
                captured_at DATETIME NOT NULL,
                event_type NVARCHAR(40) NOT NULL,
                captured_by NVARCHAR(40) NOT NULL,
                json_data NVARCHAR(MAX) NOT NULL,
                stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
                CONSTRAINT PK_{storageContext}event PRIMARY KEY (domain, stream_type, stream_id, offset),
                CONSTRAINT CK_{storageContext}event_domain_not_empty CHECK (LEN(domain) > 0),
                CONSTRAINT CK_{storageContext}event_stream_type_not_empty CHECK (LEN(stream_type) > 0),
                CONSTRAINT CK_{storageContext}event_stream_id_not_empty CHECK (LEN(stream_id) > 0),
                CONSTRAINT CK_{storageContext}event_event_type_not_empty CHECK (LEN(event_type) > 0),
                CONSTRAINT CK_{storageContext}event_captured_by_not_empty CHECK (LEN(captured_by) > 0),
                CONSTRAINT CK_{storageContext}event_json_data_not_empty CHECK (LEN(json_data) > 0)
            );

            -- Index for getting distinct values for each column domain
            CREATE INDEX IX_event_domain
            ON {storageContext}event (domain);

            -- Index for getting distinct values for columns domain and stream_type together
            CREATE INDEX stream_type
            ON {storageContext}event (domain, stream_type);

            -- Index for getting distinct values for columns domain, stream_type, and stream_id together
            CREATE INDEX stream_id
            ON {storageContext}event (domain, stream_type, stream_id);

            -- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
            CREATE INDEX IX_event_event_type_captured_at
            ON {storageContext}event (event_type, captured_at);



            -- Create the snapshot table
            CREATE TABLE {storageContext}snapshot (
                domain NVARCHAR(40) NOT NULL,
                stream_type NVARCHAR(40) NOT NULL,
                stream_id NVARCHAR(40) NOT NULL,
                offset BIGINT NOT NULL,
                json_data NVARCHAR(MAX) NOT NULL,
                stored_at DATETIME DEFAULT GETDATE() NOT NULL,
    
                CONSTRAINT PK_{storageContext}snapshot PRIMARY KEY (domain, stream_type, stream_id, offset),
                CONSTRAINT CK_{storageContext}snapshot_domain_not_empty CHECK (LEN(domain) > 0),
                CONSTRAINT CK_{storageContext}snapshot_stream_type_not_empty CHECK (LEN(stream_type) > 0),
                CONSTRAINT CK_{storageContext}snapshot_stream_id_not_empty CHECK (LEN(stream_id) > 0),
                CONSTRAINT CK_{storageContext}snapshot_json_data_not_empty CHECK (LEN(json_data) > 0)
            );

            -- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
            CREATE INDEX IX_snapshot_earlier_stored_at
            ON {storageContext}snapshot (domain, stream_type, stream_id, stored_at);
            """
        };
    }
}
