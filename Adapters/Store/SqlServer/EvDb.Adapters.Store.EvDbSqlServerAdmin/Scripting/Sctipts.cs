using EvDb.Core;
using System.Text;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Adapters.Store.SqlServer;

internal static class Sctipts
{
    private const int DEFAULT_TEXT_LIMIT = 100;

    public static EvDbAdminQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";
        string tblInitialWithoutSchema = $"{storageContext.Schema}_{storageContext.ShortId}";
        string db = storageContext.DatabaseName;

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

        // TODO: [bnaya 2025-06-03] Consider having SP for the GetMessages: https://claude.ai/public/artifacts/a06e8294-d482-421b-bf3e-ace5a01b05b3

        string destroyEnvironment = destroyEnvironmentBuilder.ToString();

        #endregion //  string destroyEnvironment = ...

        #region string eventsTableType = ...

        string eventsTableType = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $$"""
        CREATE TYPE {{tblInitial}}EventsTableType AS TABLE (        
                {{Fields.Event.Id}} UNIQUEIDENTIFIER NOT NULL,
                {{Fields.Event.StreamType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Event.StreamId}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Event.Offset}} BIGINT NOT NULL,
                {{Fields.Event.EventType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Event.CapturedBy}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Event.CapturedAt}} datetimeoffset NOT NULL,
                {{Fields.Event.TelemetryContext}} VARBINARY(2000) NULL,
                {{Fields.Event.Payload}} VARBINARY(4000) NOT NULL
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
                        {Fields.Event.Id},
                        {Fields.Event.StreamType},
                        {Fields.Event.StreamId},
                        {Fields.Event.Offset},
                        {Fields.Event.EventType},
                        {Fields.Event.CapturedBy},
                        {Fields.Event.CapturedAt},
                        {Fields.Event.TelemetryContext},
                        {Fields.Event.Payload}
                    )
                    SELECT  {Fields.Event.Id},
                            {Fields.Event.StreamType},
                            {Fields.Event.StreamId},
                            {Fields.Event.Offset},
                            {Fields.Event.EventType},
                            {Fields.Event.CapturedBy},
                            {Fields.Event.CapturedAt},
                            {Fields.Event.TelemetryContext},
                            {Fields.Event.Payload}
                        FROM @Records
                END;

            """;

        #endregion //  string createEventsBatchSP = ...

        #region string createEventsTable = ...

        string createEventsTable = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}events (
                {Fields.Event.Id} UNIQUEIDENTIFIER NOT NULL,
                {Fields.Event.StreamType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.StreamId} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.Offset} BIGINT NOT NULL,
                {Fields.Event.EventType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.CapturedBy} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.CapturedAt} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {Fields.Event.TelemetryContext} VARBINARY(2000) NULL,
                {Fields.Event.Payload} VARBINARY(4000) NOT NULL,
    
                CONSTRAINT PK_{tblInitialWithoutSchema}event PRIMARY KEY (
                        {Fields.Event.StreamType}, 
                        {Fields.Event.StreamId}, 
                        {Fields.Event.Offset}),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_root_address_not_empty CHECK (LEN({Fields.Event.StreamType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_stream_id_not_empty CHECK (LEN({Fields.Event.StreamId}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_event_type_not_empty CHECK (LEN({Fields.Event.EventType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_captured_by_not_empty CHECK (LEN({Fields.Event.CapturedBy}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_json_data_not_empty CHECK (LEN({Fields.Event.Payload}) > 0)
            );

            -- Index for getting distinct values for columns root_address, stream_type, and stream_id together
            CREATE INDEX IX_event_{tblInitialWithoutSchema}
            ON {tblInitial}events (
                    {Fields.Event.StreamType}, 
                    {Fields.Event.Offset})
            WITH (ONLINE = ON);

            -- Index for getting records with a specific value in column event_type and a value of captured_at within a given time range, sorted by captured_at
            CREATE INDEX IX_event_stored_at_{tblInitialWithoutSchema}
            ON  {tblInitial}events (stored_at)
            WITH (ONLINE = ON);

            """;

        #endregion //  string createEventsTable = ...

        #region string outboxTableType = ...

        string outboxTableType = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? string.Empty
            : $$"""
        CREATE TYPE {{tblInitial}}OutboxTableType AS TABLE (   
                {{Fields.Message.Id}} UNIQUEIDENTIFIER  NOT NULL,     
                {{Fields.Message.StreamType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.StreamId}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.Offset}} BIGINT NOT NULL,
                {{Fields.Message.EventType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.Channel}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.MessageType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.SerializeType}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.CapturedBy}} NVARCHAR({{DEFAULT_TEXT_LIMIT}}) NOT NULL,
                {{Fields.Message.CapturedAt}} datetimeoffset NOT NULL,
                {{Fields.Message.TelemetryContext}} VARBINARY(2000) NULL,
                {{Fields.Message.Payload}} VARBINARY(4000) NOT NULL
            );
        """;

        #endregion //  string outboxTableType = ...

        #region string createOutbox = ...

        IEnumerable<string> createOutbox = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? Array.Empty<string>()
            : outboxShardNames.Select(t =>
            $"""

            CREATE TABLE {tblInitial}{t} (
                {Fields.Message.Id} UNIQUEIDENTIFIER  NOT NULL, 
                {Fields.Message.StreamType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.StreamId} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.Offset} BIGINT NOT NULL,
                {Fields.Message.EventType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.Channel} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.MessageType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.SerializeType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.CapturedBy} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.CapturedAt} datetimeoffset NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
                {Fields.Message.TelemetryContext} VARBINARY(2000) NULL,
                {Fields.Message.Payload} VARBINARY(4000) NOT NULL,
            
                CONSTRAINT PK_{tblInitialWithoutSchema}{t} PRIMARY KEY (
                        {Fields.Message.StreamType}, 
                        {Fields.Message.StreamId}, 
                        {Fields.Message.Offset},
                        {Fields.Message.Channel},
                        {Fields.Message.MessageType}),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_root_address_not_empty CHECK (LEN({Fields.Message.StreamType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_stream_id_not_empty CHECK (LEN({Fields.Message.StreamId}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_event_type_not_empty CHECK (LEN({Fields.Message.EventType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_outbox_type_not_empty CHECK (LEN({Fields.Message.Channel}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_message_type_not_empty CHECK (LEN({Fields.Message.MessageType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_captured_by_not_empty CHECK (LEN({Fields.Message.CapturedBy}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}{t}_json_data_not_empty CHECK (LEN({Fields.Message.Payload}) > 0)
            );
            
            CREATE INDEX IX_{t}_{Fields.Message.Channel}_{tblInitialWithoutSchema}
               ON {tblInitial}{t} (
                     {Fields.Message.StoredAt},
                     {Fields.Message.Channel},  
                     {Fields.Message.MessageType},  
                     {Fields.Message.Offset})
            WITH (ONLINE = ON);            
            """);

        #endregion //  string createOutbox = ...

        #region IEnumerable<string> createOutboxSP = ...

        IEnumerable<string> createOutboxSP = outboxShardNames.Select(t =>
            $"""
            ------------------ Insert Message Batch SP --------------------
            CREATE PROCEDURE {tblInitial}InsertOutboxBatch_{t}
                        @{t}Records {tblInitial}OutboxTableType READONLY
                AS
                BEGIN
                    INSERT INTO {tblInitial}{t} (                           
                        {Fields.Message.Id},
                        {Fields.Message.StreamType},
                        {Fields.Message.StreamId},
                        {Fields.Message.Offset},
                        {Fields.Message.EventType},
                        {Fields.Message.Channel},
                        {Fields.Message.MessageType},
                        {Fields.Message.SerializeType},
                        {Fields.Message.CapturedBy},
                        {Fields.Message.CapturedAt},
                        {Fields.Message.TelemetryContext},
                        {Fields.Message.Payload}
                    )
                    SELECT  {Fields.Message.Id},
                            {Fields.Message.StreamType},
                            {Fields.Message.StreamId},
                            {Fields.Message.Offset},
                            {Fields.Message.EventType},
                            {Fields.Message.Channel},
                            {Fields.Message.MessageType},
                            {Fields.Message.SerializeType},
                            {Fields.Message.CapturedBy},
                            {Fields.Message.CapturedAt},
                            {Fields.Message.TelemetryContext},
                            {Fields.Message.Payload}
                        FROM @{t}Records
                END;

            """);

        #endregion //  IEnumerable<string> createOutboxSP = ...

        #region string createSnapshotTable = ...

        string createSnapshotTable = (features & StorageFeatures.Snapshot) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}snapshot (
                {Fields.Snapshot.Id} UNIQUEIDENTIFIER  NOT NULL,
                {Fields.Snapshot.StreamType} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Snapshot.StreamId} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Snapshot.ViewName} NVARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Snapshot.Offset} BIGINT NOT NULL,
                {Fields.Snapshot.State} VARBINARY(8000) NOT NULL,
                stored_at datetimeoffset DEFAULT SYSDATETIMEOFFSET() NOT NULL,
    
                CONSTRAINT PK_{tblInitialWithoutSchema}snapshot PRIMARY KEY (
                            {Fields.Snapshot.StreamType},  
                            {Fields.Snapshot.StreamId}, 
                            {Fields.Snapshot.ViewName},
                            {Fields.Snapshot.Offset}),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_root_address_not_empty CHECK (LEN({Fields.Snapshot.StreamType}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_stream_id_not_empty CHECK (LEN({Fields.Snapshot.StreamId}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_aggregate_type_not_empty CHECK (LEN({Fields.Snapshot.ViewName}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_json_data_not_empty CHECK (LEN({Fields.Snapshot.State}) > 0)
            );

            -- Index for finding records with an earlier point in time value in column stored_at than some given value, and that other records in the group exist
            CREATE INDEX IX_snapshot_earlier_stored_at_{tblInitialWithoutSchema}
            ON {tblInitial}snapshot (
                {Fields.Snapshot.StreamType}, 
                {Fields.Snapshot.StreamId},
                {Fields.Snapshot.ViewName}, stored_at)
            WITH (ONLINE = ON);

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

        var result = new EvDbAdminQueryTemplates
        {
            DestroyEnvironment = destroyEnvironment,
            CreateEnvironment = GetCreateQueries().ToArray(),
        };

        return result;
    }
}
