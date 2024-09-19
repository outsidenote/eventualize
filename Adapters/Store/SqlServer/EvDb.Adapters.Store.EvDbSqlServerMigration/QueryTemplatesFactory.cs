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
            DROP TABLE {storageContext}topic;            
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


            ------------------------------------  TOPICS  ----------------------------------------

            -- Create the topic table
            CREATE TABLE {storageContext}topic (
                {toSnakeCase(nameof(EvDbMessageRecord.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.EventType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Topic))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.MessageType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Payload))} NVARCHAR(MAX) NOT NULL,
            
                CONSTRAINT PK_{storageContext}topic PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbMessageRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.StreamId))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.Offset))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Topic))},
                        {toSnakeCase(nameof(EvDbMessageRecord.MessageType))}),
                CONSTRAINT CK_{storageContext}topic_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Domain))}) > 0),
                CONSTRAINT CK_{storageContext}topic_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Partition))}) > 0),
                CONSTRAINT CK_{storageContext}topic_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.StreamId))}) > 0),
                CONSTRAINT CK_{storageContext}topic_event_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.EventType))}) > 0),
                -- CONSTRAINT CK_{storageContext}topic_topic_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Topic))}) > 0),
                CONSTRAINT CK_{storageContext}topic_message_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.MessageType))}) > 0),
                CONSTRAINT CK_{storageContext}topic_captured_by_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{storageContext}topic_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Payload))}) > 0)
            );
            
            -- CREATE INDEX IX_topic_{toSnakeCase(nameof(EvDbMessageRecord.Topic))}
            -- ON {storageContext}topic (
            --         {toSnakeCase(nameof(EvDbMessageRecord.Topic))},
            --         {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},  
            --         {toSnakeCase(nameof(EvDbMessageRecord.Offset))});
            -- 
            -- CREATE INDEX IX_topic_{toSnakeCase(nameof(EvDbMessageRecord.Topic))}_{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}
            -- ON {storageContext}topic (
            --         {toSnakeCase(nameof(EvDbMessageRecord.Topic))}, 
            --         {toSnakeCase(nameof(EvDbMessageRecord.MessageType))},
            --         {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},  
            --         {toSnakeCase(nameof(EvDbMessageRecord.Offset))});
            -- 
            -- CREATE INDEX IX_topic_{toSnakeCase(nameof(EvDbMessageRecord.Domain))}_{toSnakeCase(nameof(EvDbMessageRecord.Partition))}_{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}_{toSnakeCase(nameof(EvDbMessageRecord.EventType))}
            -- ON {storageContext}topic (
            --         {toSnakeCase(nameof(EvDbMessageRecord.Domain))}, 
            --         {toSnakeCase(nameof(EvDbMessageRecord.Partition))},
            --         {toSnakeCase(nameof(EvDbMessageRecord.Topic))}, 
            --         {toSnakeCase(nameof(EvDbMessageRecord.MessageType))}, 
            --         {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}, 
            --         {toSnakeCase(nameof(EvDbMessageRecord.Offset))});
            
            CREATE INDEX IX_topic_{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}
            ON {storageContext}topic (
                    {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))});
            
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
