using EvDb.Core;
using EvDb.Core.Adapters;
using System.Text;

namespace EvDb.Adapters.Store.SqlServer;

internal static class QueryTemplatesFactory
{
    private const int DEFAULT_TEXT_LIMIT = 100;

    public static EvDbMigrationQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        string schema = storageContext.Schema.HasValue
            ? string.Empty
            : $"{storageContext.Schema}.";
        string tblInitial = $"{schema}{storageContext.Id}";
        string tblInitialWithoutSchema = $"{storageContext.Schema}_{storageContext.ShortId}";
        string db = storageContext.DatabaseName;
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        if (!outboxShardNames.Any())
            outboxShardNames = [EvDbShardName.Default];

        #region string destroyEnvironment = ...

        IEnumerable<string> dropOutboxTablesAndSP = outboxShardNames.Select(t => $"""
            DROP TABLE IF EXISTS {tblInitial}{t};
            DROP PROCEDURE IF EXISTS {tblInitial}InsertOutboxBatch_{t};
            """);

        StringBuilder destroyEnvironmentBuilder = new();
        destroyEnvironmentBuilder.AppendLine($"USE {db}");
        if ((features & StorageFeatures.Stream) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine($"""            
                DROP TABLE IF EXISTS  {tblInitial}events;
                DROP PROCEDURE IF EXISTS {tblInitial}InsertEventsBatch_Events
                DROP TYPE IF EXISTS {tblInitial}EventsTableType;
                """);
        }
        if ((features & StorageFeatures.Snapshot) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine($"""            
                DROP TABLE IF EXISTS  {tblInitial}snapshot;            
                """);
        }
        if ((features & StorageFeatures.Outbox) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine($"""            
                {string.Join(string.Empty, dropOutboxTablesAndSP)}            
                DROP TYPE IF EXISTS {tblInitial}OutboxTableType;
                """);
        }

        string destroyEnvironment = destroyEnvironmentBuilder.ToString();

        #endregion //  string destroyEnvironment = ...

        #region string eventsTableType = ...

        string eventsTableType = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $$"""
        CREATE TYPE {{tblInitial}}EventsTableType AS TABLE (        
                {{toSnakeCase(nameof(EvDbEventRecord.Id))}} UNIQUEIDENTIFIER NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.Domain))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.Partition))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.StreamId))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.Offset))}} BIGINT NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.EventType))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.SpanId))}} VARCHAR(16) NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.TraceId))}} VARCHAR(32) NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}} datetimeoffset NOT NULL,
                {{toSnakeCase(nameof(EvDbEventRecord.Payload))}} VARBINARY(4000) NOT NULL
            );
        """;

        #endregion //  string eventsTableType = ...

        #region string createEventsBatchSP = ...

        string createEventsBatchSP = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $"""
            -------------------------- Insert Event Batch SP --------------------------------
            CREATE PROCEDURE {tblInitial}InsertEventsBatch_Events
                        @Records {tblInitial}EventsTableType READONLY
                AS
                BEGIN
                    INSERT INTO {tblInitial}events (                           
                        {toSnakeCase(nameof(EvDbEventRecord.Id))},
                        {toSnakeCase(nameof(EvDbEventRecord.Domain))},
                        {toSnakeCase(nameof(EvDbEventRecord.Partition))},
                        {toSnakeCase(nameof(EvDbEventRecord.StreamId))},
                        {toSnakeCase(nameof(EvDbEventRecord.Offset))},
                        {toSnakeCase(nameof(EvDbEventRecord.EventType))},
                        {toSnakeCase(nameof(EvDbEventRecord.SpanId))},
                        {toSnakeCase(nameof(EvDbEventRecord.TraceId))},
                        {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))},
                        {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))},
                        {toSnakeCase(nameof(EvDbEventRecord.Payload))}
                    )
                    SELECT  {toSnakeCase(nameof(EvDbEventRecord.Id))},
                            {toSnakeCase(nameof(EvDbEventRecord.Domain))},
                            {toSnakeCase(nameof(EvDbEventRecord.Partition))},
                            {toSnakeCase(nameof(EvDbEventRecord.StreamId))},
                            {toSnakeCase(nameof(EvDbEventRecord.Offset))},
                            {toSnakeCase(nameof(EvDbEventRecord.EventType))},
                            {toSnakeCase(nameof(EvDbEventRecord.SpanId))},
                            {toSnakeCase(nameof(EvDbEventRecord.TraceId))},
                            {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))},
                            {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))},
                            {toSnakeCase(nameof(EvDbEventRecord.Payload))}
                        FROM @Records
                END;

            """;

        #endregion //  string createEventsBatchSP = ...

        #region string createEventsTable = ...

        string createEventsTable = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}events (
                {toSnakeCase(nameof(EvDbEventRecord.Id))} UNIQUEIDENTIFIER NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.EventType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.SpanId))} VARCHAR(16) NULL,
                {toSnakeCase(nameof(EvDbEventRecord.TraceId))} VARCHAR(32) NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Payload))} VARBINARY(4000) NOT NULL,
    
                CONSTRAINT PK_{tblInitialWithoutSchema}event PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.StreamId))}, 
                        {toSnakeCase(nameof(EvDbEventRecord.Offset))}),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Domain))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Partition))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.StreamId))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_event_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.EventType))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_captured_by_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbEventRecord.Payload))}) > 0)
            );

            -- Index for getting distinct values for each column domain
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{tblInitialWithoutSchema}
            ON {tblInitial}events ({toSnakeCase(nameof(EvDbEventRecord.Domain))});

            -- Index for getting distinct values for columns domain and stream_type together
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{toSnakeCase(nameof(EvDbEventRecord.Partition))}_{tblInitialWithoutSchema}
            ON {tblInitial}events (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))},
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))});

            -- Index for getting distinct values for columns domain, stream_type, and stream_id together
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{toSnakeCase(nameof(EvDbEventRecord.Partition))}_{toSnakeCase(nameof(EvDbEventRecord.EventType))}_{tblInitialWithoutSchema}
            ON {tblInitial}events (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.EventType))});

            -- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.EventType))}_{toSnakeCase(nameof(EvDbEventRecord.CapturedAt))}_{tblInitialWithoutSchema}
            ON {tblInitial}events ({toSnakeCase(nameof(EvDbEventRecord.EventType))}, {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))});

            """;

        #endregion //  string createEventsTable = ...

        #region string outboxTableType = ...

        string outboxTableType = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? string.Empty
            : $$"""
        CREATE TYPE {{tblInitial}}OutboxTableType AS TABLE (   
                {{toSnakeCase(nameof(EvDbMessageRecord.Id))}} UNIQUEIDENTIFIER  NOT NULL,     
                {{toSnakeCase(nameof(EvDbMessageRecord.Domain))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.Partition))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.StreamId))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.Offset))}} BIGINT NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.EventType))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.Channel))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.MessageType))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.SerializeType))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.SpanId))}} VARCHAR(16) NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.TraceId))}} VARCHAR(32) NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}} datetimeoffset NOT NULL,
                {{toSnakeCase(nameof(EvDbMessageRecord.Payload))}} VARBINARY(4000) NOT NULL
            );
        """;

        #endregion //  string outboxTableType = ...

        #region string createOutbox = ...

        IEnumerable<string> createOutbox = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? Array.Empty<string>()
            : outboxShardNames.Select(t =>
            $"""

            CREATE TABLE {tblInitial}{t} (
                {toSnakeCase(nameof(EvDbMessageRecord.Id))} UNIQUEIDENTIFIER  NOT NULL, 
                {toSnakeCase(nameof(EvDbMessageRecord.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.EventType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Channel))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.MessageType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.SerializeType))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.SpanId))} VARCHAR(16) NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.TraceId))} VARCHAR(32) NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Payload))} VARBINARY(4000) NOT NULL,
            
                CONSTRAINT PK_{tblInitialWithoutSchema}{t} PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.StreamId))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.Offset))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                        {toSnakeCase(nameof(EvDbMessageRecord.MessageType))}),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Domain))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Partition))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.StreamId))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_event_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.EventType))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_outbox_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Channel))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_message_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.MessageType))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_captured_by_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbMessageRecord.Payload))}) > 0)
            );
            
            CREATE INDEX IX_{t}_{toSnakeCase(nameof(EvDbMessageRecord.Channel))}_{tblInitialWithoutSchema}
               ON {tblInitial}{t} (
                     {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                     {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},  
                     {toSnakeCase(nameof(EvDbMessageRecord.Offset))});
            
            CREATE INDEX IX_{t}_{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}_{tblInitialWithoutSchema}
            ON {tblInitial}{t} (
                    {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))});

            """);

        #endregion //  string createOutbox = ...

        #region IEnumerable<string> createOutboxSP = ...

        IEnumerable<string> createOutboxSP = outboxShardNames.Select(t =>
            $"""
            ------------------ Insert Outbox Batch SP --------------------
            CREATE PROCEDURE {tblInitial}InsertOutboxBatch_{t}
                        @{t}Records {tblInitial}OutboxTableType READONLY
                AS
                BEGIN
                    INSERT INTO {tblInitial}{t} (                           
                        {toSnakeCase(nameof(EvDbMessageRecord.Id))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Domain))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Partition))},
                        {toSnakeCase(nameof(EvDbMessageRecord.StreamId))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Offset))},
                        {toSnakeCase(nameof(EvDbMessageRecord.EventType))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                        {toSnakeCase(nameof(EvDbMessageRecord.MessageType))},
                        {toSnakeCase(nameof(EvDbMessageRecord.SerializeType))},
                        {toSnakeCase(nameof(EvDbMessageRecord.SpanId))},
                        {toSnakeCase(nameof(EvDbMessageRecord.TraceId))},
                        {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))},
                        {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Payload))}
                    )
                    SELECT  {toSnakeCase(nameof(EvDbMessageRecord.Id))},
                            {toSnakeCase(nameof(EvDbMessageRecord.Domain))},
                            {toSnakeCase(nameof(EvDbMessageRecord.Partition))},
                            {toSnakeCase(nameof(EvDbMessageRecord.StreamId))},
                            {toSnakeCase(nameof(EvDbMessageRecord.Offset))},
                            {toSnakeCase(nameof(EvDbMessageRecord.EventType))},
                            {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                            {toSnakeCase(nameof(EvDbMessageRecord.MessageType))},
                            {toSnakeCase(nameof(EvDbMessageRecord.SerializeType))},
                            {toSnakeCase(nameof(EvDbMessageRecord.SpanId))},
                            {toSnakeCase(nameof(EvDbMessageRecord.TraceId))},
                            {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))},
                            {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},
                            {toSnakeCase(nameof(EvDbMessageRecord.Payload))}
                        FROM @{t}Records
                END;

            """);

        #endregion //  IEnumerable<string> createOutboxSP = ...

        #region string createSnapshotTable = ...

        string createSnapshotTable = (features & StorageFeatures.Snapshot) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}snapshot (
                {toSnakeCase(nameof(EvDbStoredSnapshotData.Id))} UNIQUEIDENTIFIER  NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.Domain))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.Partition))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.State))} NVARCHAR(MAX) NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
    
                CONSTRAINT PK_{tblInitialWithoutSchema}snapshot PRIMARY KEY (
                            {toSnakeCase(nameof(EvDbViewAddress.Domain))},  
                            {toSnakeCase(nameof(EvDbViewAddress.Partition))},   
                            {toSnakeCase(nameof(EvDbViewAddress.StreamId))}, 
                            {toSnakeCase(nameof(EvDbViewAddress.ViewName))},
                            {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))}),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_domain_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.Domain))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_stream_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.Partition))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_stream_id_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.StreamId))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_aggregate_type_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbViewAddress.ViewName))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_json_data_not_empty CHECK (LEN({toSnakeCase(nameof(EvDbStoredSnapshot.State))}) > 0)
            );

            -- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
            CREATE INDEX IX_snapshot_earlier_stored_at_{tblInitialWithoutSchema}
            ON {tblInitial}snapshot (
                {toSnakeCase(nameof(EvDbViewAddress.Domain))}, 
                {toSnakeCase(nameof(EvDbViewAddress.Partition))}, 
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))},
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))}, stored_at);

            ALTER DATABASE {db} 
            SET ALLOW_SNAPSHOT_ISOLATION ON;
            """;

        #endregion //  string createSnapshotTable = ...

        IEnumerable<string> GetCreateQueries()
        {
            yield return $"""
                                USE {db}
                                ------------------------------------  EVENTS  ----------------------------------------
                                {eventsTableType}
                                
                                {createEventsTable}
                                """;
            if ((features & StorageFeatures.Stream) != StorageFeatures.None)
                yield return createEventsBatchSP;

            if ((features & StorageFeatures.Outbox) != StorageFeatures.None)
            {
                yield return $"""
                                USE {db}
                                ------------------------------------  OUTBOX  ----------------------------------------
                                {outboxTableType}

                                {string.Join(string.Empty, createOutbox)}
                                """;
                foreach (string sp in createOutboxSP)
                {
                    yield return $"""
                                    {sp}
                                    """;
                }
            }
            if ((features & StorageFeatures.Snapshot) != StorageFeatures.None)
            {
                yield return $"""
                                USE {db}
                                -----------------------------------  SNAPSHOTS  ---------------------------------------
                                {createSnapshotTable}
                                """;
            }
        }

        var result = new EvDbMigrationQueryTemplates
        {
            DestroyEnvironment = destroyEnvironment,
            CreateEnvironment = GetCreateQueries().ToArray(),
        };

        return result;
    }
}
