using EvDb.Core;
using EvDb.Core.Adapters;
using System.Text;

namespace EvDb.Adapters.Store.Postgres;

internal static class QueryProvider
{
    private const int DEFAULT_TEXT_LIMIT = 100;

    public static EvDbMigrationQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        string schema = storageContext.Schema.HasValue
            ? $"{storageContext.Schema}."
            : string.Empty;
        string tblInitial = $"{schema}{storageContext.Id}";
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
                {toSnakeCase(nameof(EvDbEventRecord.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.EventType))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.SpanId))} VARCHAR(16),
                {toSnakeCase(nameof(EvDbEventRecord.TraceId))} VARCHAR(32),
                {toSnakeCase(nameof(EvDbEventRecord.CapturedBy))} VARCHAR({DEFAULT_TEXT_LIMIT}) NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.CapturedAt))} TIMESTAMPTZ NOT NULL,
                stored_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
                {toSnakeCase(nameof(EvDbEventRecord.Payload))} BYTEA NOT NULL,
    
                PRIMARY KEY (
                    {toSnakeCase(nameof(EvDbEventRecord.Domain))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Partition))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.StreamId))}, 
                    {toSnakeCase(nameof(EvDbEventRecord.Offset))}),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_domain_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.Domain))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_stream_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.Partition))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_stream_id_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.StreamId))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_event_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.EventType))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_captured_by_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.CapturedBy))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}event_json_data_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbEventRecord.Payload))}) > 0)
            );

            CREATE INDEX IX_event_{toSnakeCase(nameof(EvDbEventRecord.Domain))}_{tblInitialWithoutSchema}
            ON {tblInitial}events ({toSnakeCase(nameof(EvDbEventRecord.Domain))});
            """;

        #endregion

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
                {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))} BIGINT NOT NULL,
                {toSnakeCase(nameof(EvDbStoredSnapshot.State))} TEXT NOT NULL,
                stored_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    
                PRIMARY KEY (
                    {toSnakeCase(nameof(EvDbViewAddress.Domain))},  
                    {toSnakeCase(nameof(EvDbViewAddress.Partition))},   
                    {toSnakeCase(nameof(EvDbViewAddress.StreamId))}, 
                    {toSnakeCase(nameof(EvDbViewAddress.ViewName))},
                    {toSnakeCase(nameof(EvDbStoredSnapshot.Offset))}),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_domain_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.Domain))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_stream_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.Partition))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_stream_id_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.StreamId))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_aggregate_type_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbViewAddress.ViewName))}) > 0),
                CONSTRAINT CK_{tblInitialWithoutSchema}snapshot_json_data_not_empty CHECK (CHAR_LENGTH({toSnakeCase(nameof(EvDbStoredSnapshot.State))}) > 0)
            );

            CREATE INDEX IX_snapshot_earlier_stored_at_{tblInitialWithoutSchema}
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
