// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;

namespace EvDb.Adapters.Store.Postgres;

internal class EvDbPostgresStorageAdapter : EvDbRelationalStorageAdapter,
                                            IEvDbRecordParserFactory
{
    #region Ctor

    public EvDbPostgresStorageAdapter(ILogger logger,
        EvDbStorageContext context,
        IEvDbConnectionFactory factory, IEnumerable<IEvDbOutboxTransformer> transformers)
            : base(logger, factory, transformers)
    {
        StreamQueries = QueryProvider.CreateStreamQueries(context);
        SnapshotQueries = QueryProvider.CreateSnapshotQueries(context);
    }

    #endregion //  Ctor

    #region OnStoreStreamEventsAsync

    protected override async Task<int> OnStoreStreamEventsAsync(
        DbConnection connection,
        string query,
        EvDbEventRecord[] records,
        CancellationToken cancellationToken)
    {
        #region Parameters

        Guid[] ids = new Guid[records.Length];
        string[] domains = new string[records.Length];
        string[] partitions = new string[records.Length];
        string[] streamIds = new string[records.Length];
        long[] offsets = new long[records.Length];
        string[] eventTypes = new string[records.Length];
        DateTimeOffset[] capturedAts = new DateTimeOffset[records.Length];
        string[] capturedBys = new string[records.Length];
        string?[] traceIds = new string[records.Length];
        string?[] spanIds = new string[records.Length];
        List<byte[]> payloads = new();

        var traceId = Activity.Current?.TraceId.ToHexString();
        var spanId = Activity.Current?.SpanId.ToHexString();

        for (int i = 0; i < records.Length; i++)
        {
            EvDbEventRecord record = records[i];
            ids[i] = record.Id;
            domains[i] = record.Domain;
            partitions[i] = record.Partition;
            streamIds[i] = record.StreamId;
            offsets[i] = record.Offset;
            eventTypes[i] = record.EventType;
            capturedBys[i] = record.CapturedBy;
            capturedAts[i] = record.CapturedAt;
            traceIds[i] = traceId;
            spanIds[i] = spanId;
            payloads.Add(record.Payload);
        }

        #endregion //  Parameters

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection); 

        #region Setup Parameters

        command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid | NpgsqlTypes.NpgsqlDbType.Array, ids);
        command.Parameters.AddWithValue("@Domain", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, domains);
        command.Parameters.AddWithValue("@Partition", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, partitions);
        command.Parameters.AddWithValue("@StreamId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamIds);
        command.Parameters.AddWithValue("@Offset", NpgsqlTypes.NpgsqlDbType.Bigint | NpgsqlTypes.NpgsqlDbType.Array, offsets);
        command.Parameters.AddWithValue("@EventType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@CapturedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz | NpgsqlTypes.NpgsqlDbType.Array, capturedAts);
        command.Parameters.AddWithValue("@CapturedBy", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, capturedBys);
        command.Parameters.AddWithValue("@TraceId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, traceIds);
        command.Parameters.AddWithValue("@SpanId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, spanIds);
        command.Parameters.AddWithValue("@Payload", NpgsqlTypes.NpgsqlDbType.Json | NpgsqlTypes.NpgsqlDbType.Array, payloads);

        #endregion //  Setup Parameters

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreStreamEventsAsync

    #region OnStoreOutboxMessagesAsync

    protected override async Task<int> OnStoreOutboxMessagesAsync(
        DbConnection connection,
        EvDbShardName shardName,
        string query,
        EvDbMessageRecord[] records,
        CancellationToken cancellationToken)
    {
        query = string.Format(query, shardName);

        #region Parameters 

        Guid[] ids = new Guid[records.Length];
        string[] domains = new string[records.Length];
        string[] partitions = new string[records.Length];
        string[] streamIds = new string[records.Length];
        long[] offsets = new long[records.Length];
        string[] eventTypes = new string[records.Length];
        string[] messageTypes = new string[records.Length];
        string[] channels = new string[records.Length];
        string[] serializationTypes = new string[records.Length];
        DateTimeOffset[] capturedAts = new DateTimeOffset[records.Length];
        string[] capturedBys = new string[records.Length];
        string?[] traceIds = new string[records.Length];
        string?[] spanIds = new string[records.Length];
        List<byte[]> payloads = new();

        var traceId = Activity.Current?.TraceId.ToHexString();
        var spanId = Activity.Current?.SpanId.ToHexString();

        for (int i = 0; i < records.Length; i++)
        {
            EvDbMessageRecord record = records[i];
            ids[i] = record.Id;
            domains[i] = record.Domain;
            partitions[i] = record.Partition;
            streamIds[i] = record.StreamId;
            offsets[i] = record.Offset;
            eventTypes[i] = record.EventType;
            capturedBys[i] = record.CapturedBy;
            capturedAts[i] = record.CapturedAt;
            messageTypes[i] = record.MessageType;
            serializationTypes[i] = record.SerializeType;
            channels[i] = record.Channel;
            traceIds[i] = traceId;
            spanIds[i] = spanId;
            payloads.Add(record.Payload);
        }

        #endregion //  Parameters 

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection);

        #region Setup Parameters

        command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid | NpgsqlTypes.NpgsqlDbType.Array, ids);
        command.Parameters.AddWithValue("@Domain", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, domains);
        command.Parameters.AddWithValue("@Partition", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, partitions);
        command.Parameters.AddWithValue("@StreamId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamIds);
        command.Parameters.AddWithValue("@Offset", NpgsqlTypes.NpgsqlDbType.Bigint | NpgsqlTypes.NpgsqlDbType.Array, offsets);
        command.Parameters.AddWithValue("@Channel", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, channels);
        command.Parameters.AddWithValue("@MessageType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, messageTypes);
        command.Parameters.AddWithValue("@SerializeType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, serializationTypes);
        command.Parameters.AddWithValue("@EventType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@CapturedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz | NpgsqlTypes.NpgsqlDbType.Array, capturedAts);
        command.Parameters.AddWithValue("@CapturedBy", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, capturedBys);
        command.Parameters.AddWithValue("@TraceId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, traceIds);
        command.Parameters.AddWithValue("@SpanId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, spanIds);
        command.Parameters.AddWithValue("@Payload", NpgsqlTypes.NpgsqlDbType.Bytea | NpgsqlTypes.NpgsqlDbType.Array, payloads);

        #endregion // Setup Parameters

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreOutboxMessagesAsync

    #region OnStoreSnapshotAsync

    protected override async Task<int> OnStoreSnapshotAsync(DbConnection connection,
                                                      string query,
                                                      EvDbStoredSnapshotData snapshot,
                                                      CancellationToken cancellationToken)
    {
        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection);

        #region Setup Parameters

        command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid, snapshot.Id);
        command.Parameters.AddWithValue("@Domain", NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.Domain);
        command.Parameters.AddWithValue("@Partition", NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.Partition);
        command.Parameters.AddWithValue("@StreamId", NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.StreamId);
        command.Parameters.AddWithValue("@ViewName", NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.ViewName);
        command.Parameters.AddWithValue("@Offset", NpgsqlTypes.NpgsqlDbType.Bigint, snapshot.Offset);
        command.Parameters.AddWithValue("@State", NpgsqlTypes.NpgsqlDbType.Json, snapshot.State);

        #endregion //  Setup Parameters

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreSnapshotAsync

    #region OnGetSnapshotAsync

    protected async override Task<EvDbStoredSnapshot> OnGetSnapshotAsync(
                                                            EvDbViewAddress viewAddress,
                                                            DbConnection conn,
                                                            string query,
                                                            CancellationToken cancellation)
    {
        var command = new NpgsqlCommand(query, (NpgsqlConnection)conn);
        command.Parameters.AddWithValue(nameof(EvDbViewAddress.Domain), viewAddress.Domain);
        command.Parameters.AddWithValue(nameof(EvDbViewAddress.Partition), viewAddress.Partition);
        command.Parameters.AddWithValue(nameof(EvDbViewAddress.StreamId), viewAddress.StreamId);
        command.Parameters.AddWithValue(nameof(EvDbViewAddress.ViewName), viewAddress.ViewName);

        NpgsqlDataReader reader =
            await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellation);
        if (!await reader.ReadAsync())
            return EvDbStoredSnapshot.Empty;

        var stateIndex = reader.GetOrdinal(nameof(EvDbStoredSnapshot.State));
        var offset = reader.GetInt64(reader.GetOrdinal(nameof(EvDbStoredSnapshot.Offset)));
        var state = Encoding.UTF8.GetBytes(reader.GetString(stateIndex));
        var record = new EvDbStoredSnapshot(offset, state);
        return record;

    }

    #endregion //  OnGetSnapshotAsync

    #region IEvDbRecordParserFactory members

    /// <summary>
    /// Creates the specified reader.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns></returns>
    IEvDbRecordParser IEvDbRecordParserFactory.Create(DbDataReader reader) => new RecordParser(reader);

    #endregion //  IEvDbRecordParserFactory members

    #region class RecordParser

    private sealed class RecordParser : IEvDbRecordParser
    {
        private readonly DbDataReader _reader;

        public RecordParser(DbDataReader reader)
        {
            _reader = reader;
        }

        EvDbEventRecord IEvDbRecordParser.ParseEvent()
        {
            var payloadIndex = _reader.GetOrdinal(nameof(EvDbEventRecord.Payload));
            var record = new EvDbEventRecord
            {
                Domain = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.Domain))), // Non-nullable
                Partition = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.Partition))), // Non-nullable
                StreamId = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.StreamId))), // Non-nullable
                Offset = _reader.GetInt64(_reader.GetOrdinal(nameof(EvDbEventRecord.Offset))), // Non-nullable
                EventType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.EventType))), // Non-nullable
                CapturedBy = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.CapturedBy))), // Non-nullable
                CapturedAt = _reader.GetDateTime(_reader.GetOrdinal(nameof(EvDbEventRecord.CapturedAt))), // Non-nullable
                Payload = Encoding.UTF8.GetBytes(_reader.GetString(payloadIndex))
            };
            return record;
        }
    }

    #endregion //  class RecordParser

    protected override string DatabaseType { get; } = "sql-server";

    protected override EvDbStreamAdapterQueryTemplates StreamQueries { get; }

    protected override EvDbSnapshotAdapterQueryTemplates SnapshotQueries { get; }

    protected override bool IsSupportConcurrentCommands { get; } = false;

    #region IsOccException

    protected override bool IsOccException(Exception exception)
    {
        bool result = exception is PostgresException postgresException &&
                 postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
        return result;
    }

    #endregion //  IsOccException
}
