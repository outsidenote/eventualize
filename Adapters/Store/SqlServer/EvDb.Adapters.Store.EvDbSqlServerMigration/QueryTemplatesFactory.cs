using EvDb.Core;
using EvDb.Core.Adapters;

namespace EvDb.Adapters.Store.SqlServer;

internal static class QueryTemplatesFactory
{
    private const int DEFAULT_TEXT_LIMIT = 100;

    public static EvDbMigrationQueryTemplates Create(EvDbStorageContext storageContext, string dbName = "master")
    {
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        return new EvDbMigrationQueryTemplates
        {
            DestroyEnvironment = $"""
            DROP TABLE {storageContext}event;
            DROP TABLE {storageContext}outbox;            
            DROP TABLE {storageContext}snapshot;            
            """,
            CreateEnvironment = $"""
            ----------------------------------- EVENT -------------------------------
            
            -- Create the event table
            CREATE TABLE {storageContext}event (
                {toSnakeCase(nameof(EvDbEventRecord.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.EventType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Payload))} NVARCHAR(MAX) NOT NULL,
    
                CONSTRAINT PK_{storageContext}event PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.StreamId))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.Offset))}),
                CONSTRAINT CK_{storageContext}event_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Domain))}) > 0),
                CONSTRAINT CK_{storageContext}event_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Partition))}) > 0),
                CONSTRAINT CK_{storageContext}event_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.StreamId))}) > 0),
                CONSTRAINT CK_{storageContext}event_event_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.EventType))}) > 0),
                CONSTRAINT CK_{storageContext}event_captured_by_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{storageContext}event_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Payload))}) > 0)
            );

            -- Index for getting distinct values for each column domain
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}
            ON {storageContext}event ({toSnakeCase(nameof(EvDbEventRecord.Domain))});

            -- Index for getting distinct values for columns domain and stream_type together
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{toSnakeCase(nameof(EvDbEventRecord.Partition))}
            ON {storageContext}event (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))},
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))});

            -- Index for getting distinct values for columns domain, stream_type, and stream_id together
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{toSnakeCase(nameof(EvDbEventRecord.Partition))}_{toSnakeCase(nameof(EvDbEventRecord.EventType))}
            ON {storageContext}event (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.EventType))});

            -- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.EventType))}_{toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}
            ON {storageContext}event ({toSnakeCase(nameof(EvDbEventRecord.EventType))}, {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))});


            ------------------------------------  OUTBOX  ----------------------------------------

            -- Create the outbox table
            CREATE TABLE {storageContext}outbox (
                {toSnakeCase(nameof(EvDbOutboxRecord.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.EventType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.CapturedBy))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.CapturedAt))} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {toSnakeCase(nameof(EvDbOutboxRecord.Payload))} NVARCHAR(MAX) NOT NULL,
            
                CONSTRAINT PK_{storageContext}outbox PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbOutboxRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbOutboxRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbOutboxRecord.StreamId))}, 
                        {toSnakeCase(nameof(EvDbOutboxRecord.Offset))},
                        {toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))}),
                CONSTRAINT CK_{storageContext}outbox_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.Domain))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.Partition))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.StreamId))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_event_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.EventType))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_outbox_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_captured_by_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{storageContext}outbox_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbOutboxRecord.Payload))}) > 0)
            );
            
            CREATE INDEX IX_outbox_{toSnakeCase(nameof(EvDbOutboxRecord.Domain))}_{toSnakeCase(nameof(EvDbOutboxRecord.Partition))}_{toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))}_{toSnakeCase(nameof(EvDbOutboxRecord.EventType))}
            ON {storageContext}outbox (
                    {toSnakeCase(nameof(EvDbOutboxRecord.CapturedAt))}, 
                    {toSnakeCase(nameof(EvDbOutboxRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbOutboxRecord.Partition))},
                    {toSnakeCase(nameof(EvDbOutboxRecord.OutboxType))}, 
                    {toSnakeCase(nameof(EvDbOutboxRecord.EventType))});

            CREATE INDEX IX_outbox_{toSnakeCase(nameof(EvDbOutboxRecord.CapturedAt))}
            ON {storageContext}outbox (
                    {toSnakeCase(nameof(EvDbOutboxRecord.CapturedAt))});
            
            ------------------------------------------------  SNAPSHOT ---------------------------------------
            
            -- Create the snapshot table
            CREATE TABLE {storageContext}snapshot (
                {toSnakeCase(nameof(EvDbViewAddress.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.State))} NVARCHAR(MAX) NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
    
                CONSTRAINT PK_{storageContext}snapshot PRIMARY KEY (
                            {toSnakeCase(nameof(EvDbViewAddress.Domain))},  
                            {toSnakeCase(nameof(EvDbViewAddress.Partition))},   
                            {toSnakeCase(nameof(EvDbViewAddress.StreamId))}, 
                            {toSnakeCase(nameof(EvDbViewAddress.ViewName))},
                            {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))}),
                CONSTRAINT CK_{storageContext}snapshot_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.Domain))}) > 0),
                CONSTRAINT CK_{storageContext}snapshot_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.Partition))}) > 0),
                CONSTRAINT CK_{storageContext}snapshot_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.StreamId))}) > 0),
                CONSTRAINT CK_{storageContext}snapshot_aggregate_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.ViewName))}) > 0),
                CONSTRAINT CK_{storageContext}snapshot_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbStoredSnapshot.State))}) > 0)
            );

            -- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
            CREATE INDEX IX_snapshot_earlier_stored_at
            ON {storageContext}snapshot (
                {toSnakeCase(nameof(EvDbViewAddress.Domain))}, 
                {toSnakeCase(nameof(EvDbViewAddress.Partition))}, 
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))},
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))}, stored_at);

            ALTER DATABASE {dbName} SET ALLOW_SNAPSHOT_ISOLATION ON;
            """
        };

    }
}
