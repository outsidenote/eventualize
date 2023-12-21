namespace Eventualize.Core.Adapters.PostgresStore;

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
            -- Create the event table
            CREATE TABLE {storageContext}event (
                domain VARCHAR(40) NOT NULL,
                aggregate_type VARCHAR(40) NOT NULL,
                aggregate_id VARCHAR(40) NOT NULL,
                sequence_id BIGINT NOT NULL,
                captured_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
                event_type VARCHAR(40) NOT NULL,
                captured_by VARCHAR(40) NOT NULL,
                json_data JSONB NOT NULL,
                stored_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

                CONSTRAINT PK_{storageContext}event PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
                CHECK (LENGTH(domain) > 0),
                CHECK (LENGTH(aggregate_type) > 0),
                CHECK (LENGTH(aggregate_id) > 0),
                CHECK (LENGTH(event_type) > 0),
                CHECK (LENGTH(captured_by) > 0),
                CHECK (LENGTH(json_data) > 0)
            );

            -- Index for getting distinct values for each column domain
            CREATE INDEX IX_event_domain ON {storageContext}event (domain);

            -- Index for getting distinct values for columns domain and aggregate_type together
            CREATE INDEX IX_event_domain_aggregate_type ON {storageContext}event (domain, aggregate_type);

            -- Index for getting distinct values for columns domain, aggregate_type, and aggregate_id together
            CREATE INDEX IX_event_domain_aggregate_type_aggregate_id ON {storageContext}event (domain, aggregate_type, aggregate_id);

            -- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
            CREATE INDEX IX_event_event_type_captured_at ON {storageContext}event (event_type, captured_at);

            -- Create the snapshot table
            CREATE TABLE {storageContext}snapshot (
                domain VARCHAR(40) NOT NULL,
                aggregate_type VARCHAR(40) NOT NULL,
                aggregate_id VARCHAR(40) NOT NULL,
                sequence_id BIGINT NOT NULL,
                json_data JSONB NOT NULL,
                stored_at TIMESTAMP WITHOUT TIME ZONE DEFAULT CURRENT_TIMESTAMP NOT NULL,

                CONSTRAINT PK_{storageContext}snapshot PRIMARY KEY (domain, aggregate_type, aggregate_id, sequence_id),
                CHECK (LENGTH(domain) > 0),
                CHECK (LENGTH(aggregate_type) > 0),
                CHECK (LENGTH(aggregate_id) > 0),
                CHECK (LENGTH(json_data) > 0)
            );

            -- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
            CREATE INDEX IX_snapshot_earlier_stored_at ON {storageContext}snapshot (domain, aggregate_type, aggregate_id, stored_at);            
            """
        };
    }
}
