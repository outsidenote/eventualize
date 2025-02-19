using EvDb.Core;
using EvDb.Core.Adapters;
using System.Text;

namespace EvDb.Adapters.Store.MongoDB;

internal static class QueryProvider
{
    private const int DEFAULT_TEXT_LIMIT = 100;

    public static EvDbMigrationQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        Guid unique = Guid.NewGuid();
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";
        string tblInitialWithoutSchema = $"{storageContext.Schema}_{storageContext.ShortId}";
        Func<string, string> toSnakeCase = EvDbStoreNamingPolicy.Default.ConvertName;

        if (!outboxShardNames.Any())
            outboxShardNames = new[] { EvDbShardName.Default };

        #region string destroyEnvironment = ...

        IEnumerable<string> dropOutboxTablesAndSP = outboxShardNames.Select(t => $"""
            DROP TABLE IF EXISTS {tblInitial}{t} CASCADE;
            """);

        StringBuilder destroyEnvironmentBuilder = new();
        if ((features & StorageFeatures.Stream) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine($"""            
                DROP TABLE IF EXISTS {tblInitial}events CASCADE;
                """);
        }
        if ((features & StorageFeatures.Snapshot) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine($"""            
                DROP TABLE IF EXISTS {tblInitial}snapshot CASCADE;            
                """);
        }
        if ((features & StorageFeatures.Outbox) != StorageFeatures.None)
        {
            destroyEnvironmentBuilder.AppendLine(string.Join(string.Empty, dropOutboxTablesAndSP));
        }

        string destroyEnvironment = destroyEnvironmentBuilder.ToString();

        #endregion

        #region string createEventsTable = ...

        string createEventsTable = (features & StorageFeatures.Stream) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}events (
                {toSnakeCase(nameof(EvDbEventRecord.Id))} UUID NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Domain))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Partition))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.StreamId))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{toSnakeCase(nameof(EvDbEventRecord.Offset))}" BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.EventType))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.SpanId))} VARCHAR(16),
                {toSnakeCase(nameof(EvDbEventRecord.TraceId))} VARCHAR(32),
                {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} TIMESTAMPTZ NOT NULL,
                stored_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Payload))} JSON NOT NULL,
    
                PRIMARY KEY (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.StreamId))}, 
                    "{toSnakeCase(nameof(EvDbEventRecord.Offset))}"),
                CONSTRAINT CK_event_domain_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.Domain))}) > 0),
                CONSTRAINT CK_event_stream_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.Partition))}) > 0),
                CONSTRAINT CK_event_stream_id_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.StreamId))}) > 0),
                CONSTRAINT CK_event_event_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.EventType))}) > 0),
                CONSTRAINT CK_event_captured_by_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}) > 0)
            );

            -- Index for getting distinct values for columns domain, partition, and event_type together
            CREATE INDEX ix_event_{unique:N}
            ON {tblInitial}events (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.StreamId))}, 
                    "{toSnakeCase(nameof(EvDbEventRecord.Offset))}" 
            );
            CREATE INDEX ix_event_stored_at_{unique:N}
            ON {tblInitial}events (
                    stored_at 
            );
            
            """;

        #endregion

        #region string createOutboxTable = ...

        IEnumerable<string> createOutbox = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? Array.Empty<string>()
            : outboxShardNames.Select(t =>
            $"""

            CREATE TABLE {tblInitial}{t} (
                {toSnakeCase(nameof(EvDbMessageRecord.Id))} UUID  NOT NULL, 
                {toSnakeCase(nameof(EvDbMessageRecord.Domain))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Partition))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.StreamId))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{toSnakeCase(nameof(EvDbMessageRecord.Offset))}" BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.EventType))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Channel))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.MessageType))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.SerializeType))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.SpanId))} VARCHAR(16) NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.TraceId))} VARCHAR(32) NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))} TIMESTAMPTZ NOT NULL,
                stored_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
                {toSnakeCase(nameof(EvDbMessageRecord.Payload))} BYTEA NOT NULL CHECK (octet_length({toSnakeCase(nameof(EvDbMessageRecord.Payload))}) > 0 AND octet_length({toSnakeCase(nameof(EvDbMessageRecord.Payload))}) <= 4000),
            
                PRIMARY KEY (
                        {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},
                        {toSnakeCase(nameof(EvDbMessageRecord.Domain))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.Partition))}, 
                        {toSnakeCase(nameof(EvDbMessageRecord.StreamId))}, 
                        "{toSnakeCase(nameof(EvDbMessageRecord.Offset))}",
                        {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                        {toSnakeCase(nameof(EvDbMessageRecord.MessageType))}),
                CONSTRAINT CK_{t}_domain_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.Domain))}) > 0),
                CONSTRAINT CK_{t}_stream_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.Partition))}) > 0),
                CONSTRAINT CK_{t}_stream_id_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.StreamId))}) > 0),
                CONSTRAINT CK_{t}_event_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.EventType))}) > 0),
                CONSTRAINT CK_{t}_outbox_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.Channel))}) > 0),
                CONSTRAINT CK_{t}_message_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.MessageType))}) > 0),
                CONSTRAINT CK_{t}_captured_by_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbMessageRecord.CapturedBy))}) > 0)
            );
            
            CREATE INDEX ix_{t}_{unique:N}
            ON {tblInitial}{t} (
                {toSnakeCase(nameof(EvDbMessageRecord.Channel))},
                {toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))},
                "{toSnakeCase(nameof(EvDbEventRecord.Offset))}"
            );
                        
            CREATE INDEX ix_StoredAt_{t}_{toSnakeCase(nameof(EvDbMessageRecord.CapturedAt))}_{unique:N}
            ON {tblInitial}{t} (
                    stored_at,
                    "{toSnakeCase(nameof(EvDbEventRecord.Offset))}");

            """);

        #endregion //  string createOutbox = ...

        #region string createSnapshotTable = ...

        string createSnapshotTable = (features & StorageFeatures.Snapshot) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}snapshot (
                {toSnakeCase(nameof(EvDbStoredSnapshotData.Id))} UUID NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.Domain))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.Partition))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{toSnakeCase(nameof(EvDbStoredSnapshot.Offset))}" BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.State))} JSON NOT NULL,
                stored_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
                PRIMARY KEY (
                    {toSnakeCase(nameof(EvDbViewAddress.Domain))},  
                    {toSnakeCase(nameof(EvDbViewAddress.Partition))},   
                    {toSnakeCase(nameof(EvDbViewAddress.StreamId))}, 
                    {toSnakeCase(nameof(EvDbViewAddress.ViewName))},
                    "{toSnakeCase(nameof(EvDbStoredSnapshot.Offset))}"),
                CONSTRAINT CK_snapshot_domain_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.Domain))}) > 0),
                CONSTRAINT CK_snapshot_stream_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.Partition))}) > 0),
                CONSTRAINT CK_snapshot_stream_id_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.StreamId))}) > 0),
                CONSTRAINT CK_snapshot_aggregate_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.ViewName))}) > 0)
            );

            CREATE INDEX ix_snapshot_earlier_stored_at_{unique:N}
            ON {tblInitial}snapshot (
                {toSnakeCase(nameof(EvDbViewAddress.Domain))}, 
                {toSnakeCase(nameof(EvDbViewAddress.Partition))}, 
                {toSnakeCase(nameof(EvDbViewAddress.StreamId))},
                {toSnakeCase(nameof(EvDbViewAddress.ViewName))}, stored_at);
            """;

        #endregion

        IEnumerable<string> GetCreateQueries()
        {
            yield return $"""
                ------------------------------------  EVENTS  ----------------------------------------
                {createEventsTable}
                """;

            if ((features & StorageFeatures.Outbox) != StorageFeatures.None)
            {
                yield return $"""
                                {string.Join(string.Empty, createOutbox)}
                                """;
            }

            if ((features & StorageFeatures.Snapshot) != StorageFeatures.None)
            {
                yield return $"""
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
