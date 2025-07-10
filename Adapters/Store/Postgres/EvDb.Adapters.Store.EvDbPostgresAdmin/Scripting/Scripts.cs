using EvDb.Core;
using System.Text;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

namespace EvDb.Adapters.Store.Postgres;

internal static class Scripts
{
    private const int DEFAULT_TEXT_LIMIT = 150;

    public static EvDbAdminQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        Guid unique = Guid.NewGuid();
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.ShortId}";

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
                {Fields.Event.Id} UUID NOT NULL,
                {Fields.Event.StreamType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.StreamId} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{Fields.Event.Offset}" BIGINT NOT NULL,
                {Fields.Event.EventType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.TelemetryContext} JSON NULL,
                {Fields.Event.CapturedBy} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Event.CapturedAt} TIMESTAMPTZ NOT NULL,
                {Fields.Event.StoredAt} TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
                {Fields.Event.Payload} JSON NOT NULL,
    
                PRIMARY KEY (
                    {Fields.Event.StreamType}, 
                    {Fields.Event.StreamId}, 
                    "{Fields.Event.Offset}"),
                CONSTRAINT CK_event_root_address_not_empty CHECK (CHAR_LENGTH({Fields.Event.StreamType}) > 0),
                CONSTRAINT CK_event_stream_id_not_empty CHECK (CHAR_LENGTH({Fields.Event.StreamId}) > 0),
                CONSTRAINT CK_event_event_type_not_empty CHECK (CHAR_LENGTH({Fields.Event.EventType}) > 0),
                CONSTRAINT CK_event_captured_by_not_empty CHECK (CHAR_LENGTH({Fields.Event.CapturedBy}) > 0)
            );

            -- Index for getting distinct values for columns root-address, and event_type together
            CREATE INDEX ix_event_{unique:N}
            ON {tblInitial}events (
                    {Fields.Event.StreamType}, 
                    {Fields.Event.StreamId}, 
                    "{Fields.Event.Offset}" 
            );
            CREATE INDEX ix_event_stored_at_{unique:N}
            ON {tblInitial}events (
                    {Fields.Event.StoredAt} 
            );
            
            """;

        #endregion

        #region string createOutboxTable = ...

        IEnumerable<string> createOutbox = (features & StorageFeatures.Outbox) == StorageFeatures.None
            ? Array.Empty<string>()
            : outboxShardNames.Select(t =>
            $"""

            CREATE TABLE {tblInitial}{t} (
                {Fields.Message.Id} UUID  NOT NULL, 
                {Fields.Message.StreamType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.StreamId} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{Fields.Message.Offset}" BIGINT NOT NULL,
                {Fields.Message.EventType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.Channel} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.MessageType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.SerializeType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.TelemetryContext} BYTEA NULL,
                {Fields.Message.CapturedBy} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Message.CapturedAt} TIMESTAMPTZ NOT NULL,
                {Fields.Message.StoredAt} TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
                {Fields.Message.Payload} BYTEA NOT NULL CHECK (octet_length({Fields.Message.Payload}) > 0 AND octet_length({Fields.Message.Payload}) <= 4000),
            
                PRIMARY KEY (
                        {Fields.Message.CapturedAt},
                        {Fields.Message.StreamType}, 
                        {Fields.Message.StreamId}, 
                        "{Fields.Message.Offset}",
                        {Fields.Message.Channel},
                        {Fields.Message.MessageType}),
                CONSTRAINT CK_{t}_root_address_not_empty CHECK (CHAR_LENGTH({Fields.Message.StreamType}) > 0),
                CONSTRAINT CK_{t}_stream_id_not_empty CHECK (CHAR_LENGTH({Fields.Message.StreamId}) > 0),
                CONSTRAINT CK_{t}_event_type_not_empty CHECK (CHAR_LENGTH({Fields.Message.EventType}) > 0),
                CONSTRAINT CK_{t}_outbox_type_not_empty CHECK (CHAR_LENGTH({Fields.Message.Channel}) > 0),
                CONSTRAINT CK_{t}_message_type_not_empty CHECK (CHAR_LENGTH({Fields.Message.MessageType}) > 0),
                CONSTRAINT CK_{t}_captured_by_not_empty CHECK (CHAR_LENGTH({Fields.Message.CapturedBy}) > 0)
            );
            
            CREATE INDEX ix_{t}_{unique:N}
            ON {tblInitial}{t} (
                        {Fields.Message.StreamType}, 
                        {Fields.Message.StreamId}, 
                        "{Fields.Message.Offset}",
                        {Fields.Message.Channel},
                        {Fields.Message.MessageType});
                                    
            CREATE INDEX ix_StoredAt_{t}_{Fields.Message.CapturedAt}_{unique:N}
            ON {tblInitial}{t} (
                    {Fields.Message.StoredAt},
                    {Fields.Message.Channel},
                    {Fields.Message.MessageType},
                    "{Fields.Message.Offset}");

            """);

        #endregion //  string createOutbox = ...

        #region string createSnapshotTable = ...

        string createSnapshotTable = (features & StorageFeatures.Snapshot) == StorageFeatures.None
            ? string.Empty
            : $"""
            CREATE TABLE {tblInitial}snapshot (
                {Fields.Snapshot.Id} UUID NOT NULL,
                {Fields.Snapshot.StreamType} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Snapshot.StreamId} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {Fields.Snapshot.ViewName} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                "{Fields.Snapshot.Offset}" BIGINT NOT NULL,
                {Fields.Snapshot.State} JSON NOT NULL,
                {Fields.Snapshot.StoredAt} TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
                PRIMARY KEY (
                    {Fields.Snapshot.StreamType},  
                    {Fields.Snapshot.StreamId}, 
                    {Fields.Snapshot.ViewName},
                    "{Fields.Snapshot.Offset}"),
                CONSTRAINT CK_snapshot_root_address_not_empty CHECK (CHAR_LENGTH({Fields.Snapshot.StreamType}) > 0),
                CONSTRAINT CK_snapshot_stream_id_not_empty CHECK (CHAR_LENGTH({Fields.Snapshot.StreamId}) > 0),
                CONSTRAINT CK_snapshot_aggregate_type_not_empty CHECK (CHAR_LENGTH({Fields.Snapshot.ViewName}) > 0)
            );

            CREATE INDEX ix_snapshot_earlier_stored_at_{unique:N}
            ON {tblInitial}snapshot (
                {Fields.Snapshot.StreamType}, 
                {Fields.Snapshot.StreamId},
                {Fields.Snapshot.ViewName}, 
                {Fields.Snapshot.StoredAt});
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

        var result = new EvDbAdminQueryTemplates
        {
            DestroyEnvironment = destroyEnvironment,
            CreateEnvironment = GetCreateQueries().ToArray(),
        };

        return result;
    }
}
