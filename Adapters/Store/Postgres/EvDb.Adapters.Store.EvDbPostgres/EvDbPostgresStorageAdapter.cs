// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames;

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
        string[] streamTypees = new string[records.Length];
        string[] streamIds = new string[records.Length];
        long[] offsets = new long[records.Length];
        string[] eventTypes = new string[records.Length];
        DateTimeOffset[] capturedAts = new DateTimeOffset[records.Length];
        string[] capturedBys = new string[records.Length];
        List<byte[]?> otelContexts = new();
        List<byte[]> payloads = new();

        var otelContext = Activity.Current?.SerializeTelemetryContext();
        for (int i = 0; i < records.Length; i++)
        {
            EvDbEventRecord record = records[i];
            ids[i] = record.Id;
            streamTypees[i] = record.StreamType;
            streamIds[i] = record.StreamId;
            offsets[i] = record.Offset;
            eventTypes[i] = record.EventType;
            capturedBys[i] = record.CapturedBy;
            capturedAts[i] = record.CapturedAt;
            otelContexts.Add(otelContext);
            payloads.Add(record.Payload);
        }

        #endregion //  Parameters

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection);

        #region Setup Parameters

        command.Parameters.AddWithValue(Parameters.Event.Id, NpgsqlTypes.NpgsqlDbType.Uuid | NpgsqlTypes.NpgsqlDbType.Array, ids);
        command.Parameters.AddWithValue(Parameters.Event.StreamType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamTypees);
        command.Parameters.AddWithValue(Parameters.Event.StreamId, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamIds);
        command.Parameters.AddWithValue(Parameters.Event.Offset, NpgsqlTypes.NpgsqlDbType.Bigint | NpgsqlTypes.NpgsqlDbType.Array, offsets);
        command.Parameters.AddWithValue(Parameters.Event.EventType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue(Parameters.Event.CapturedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz | NpgsqlTypes.NpgsqlDbType.Array, capturedAts);
        command.Parameters.AddWithValue(Parameters.Event.CapturedBy, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, capturedBys);
        command.Parameters.AddWithValue(Parameters.Event.TelemetryContext, NpgsqlTypes.NpgsqlDbType.Json | NpgsqlTypes.NpgsqlDbType.Array, otelContexts);
        command.Parameters.AddWithValue(Parameters.Event.Payload, NpgsqlTypes.NpgsqlDbType.Json | NpgsqlTypes.NpgsqlDbType.Array, payloads);

        #endregion //  Setup Parameters

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreStreamEventsAsync

    #region ShouldRetryOnConnectionError

    protected override bool ShouldRetryOnConnectionError(Exception exception, int retryCount) =>
        exception switch
        {
            PostgresException ex when ex.SqlState == "53300" => true,
            _ => false
        };

    #endregion //  ShouldRetryOnConnectionError

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

        var otelContext = Activity.Current?.SerializeTelemetryContext();

        Guid[] ids = new Guid[records.Length];
        string[] streamTypees = new string[records.Length];
        string[] streamIds = new string[records.Length];
        long[] offsets = new long[records.Length];
        string[] eventTypes = new string[records.Length];
        string[] messageTypes = new string[records.Length];
        string[] channels = new string[records.Length];
        string[] serializationTypes = new string[records.Length];
        DateTimeOffset[] capturedAts = new DateTimeOffset[records.Length];
        string[] capturedBys = new string[records.Length];
        List<byte[]?> otelContexts = new();
        List<byte[]> payloads = new();
        for (int i = 0; i < records.Length; i++)
        {
            EvDbMessageRecord record = records[i];
            ids[i] = record.Id;
            streamTypees[i] = record.StreamType;
            streamIds[i] = record.StreamId;
            offsets[i] = record.Offset;
            eventTypes[i] = record.EventType;
            capturedBys[i] = record.CapturedBy;
            capturedAts[i] = record.CapturedAt;
            messageTypes[i] = record.MessageType;
            serializationTypes[i] = record.SerializeType;
            channels[i] = record.Channel;
            otelContexts.Add(otelContext);
            payloads.Add(record.Payload);
        }

        #endregion //  Parameters 

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection);

        #region Setup Parameters

        command.Parameters.AddWithValue(Parameters.Message.Id, NpgsqlTypes.NpgsqlDbType.Uuid | NpgsqlTypes.NpgsqlDbType.Array, ids);
        command.Parameters.AddWithValue(Parameters.Message.StreamType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamTypees);
        command.Parameters.AddWithValue(Parameters.Message.StreamId, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamIds);
        command.Parameters.AddWithValue(Parameters.Message.Offset, NpgsqlTypes.NpgsqlDbType.Bigint | NpgsqlTypes.NpgsqlDbType.Array, offsets);
        command.Parameters.AddWithValue(Parameters.Message.Channel, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, channels);
        command.Parameters.AddWithValue(Parameters.Message.MessageType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, messageTypes);
        command.Parameters.AddWithValue(Parameters.Message.SerializeType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, serializationTypes);
        command.Parameters.AddWithValue(Parameters.Message.EventType, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue(Parameters.Message.CapturedAt, NpgsqlTypes.NpgsqlDbType.TimestampTz | NpgsqlTypes.NpgsqlDbType.Array, capturedAts);
        command.Parameters.AddWithValue(Parameters.Message.CapturedBy, NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, capturedBys);
        command.Parameters.AddWithValue(Parameters.Message.TelemetryContext, NpgsqlTypes.NpgsqlDbType.Bytea | NpgsqlTypes.NpgsqlDbType.Array, otelContexts);
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

        command.Parameters.AddWithValue(Parameters.Snapshot.Id, NpgsqlTypes.NpgsqlDbType.Uuid, snapshot.Id);
        command.Parameters.AddWithValue(Parameters.Snapshot.StreamType, NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.StreamType);
        command.Parameters.AddWithValue(Parameters.Snapshot.StreamId, NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.StreamId);
        command.Parameters.AddWithValue(Parameters.Snapshot.ViewName, NpgsqlTypes.NpgsqlDbType.Varchar, snapshot.ViewName);
        command.Parameters.AddWithValue(Parameters.Snapshot.Offset, NpgsqlTypes.NpgsqlDbType.Bigint, snapshot.Offset);
        command.Parameters.AddWithValue(Parameters.Snapshot.State, NpgsqlTypes.NpgsqlDbType.Json, snapshot.State);

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
        command.Parameters.AddWithValue(nameof(EvDbViewAddress.StreamType), viewAddress.StreamType);
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
    IEvDbRecordParser IEvDbRecordParserFactory.CreateParser(DbDataReader reader) => new RecordParser(reader);

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
                StreamType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.StreamType))), 
                StreamId = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.StreamId))), 
                Offset = _reader.GetInt64(_reader.GetOrdinal(nameof(EvDbEventRecord.Offset))), 
                EventType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.EventType))), 
                CapturedBy = _reader.GetString(_reader.GetOrdinal(nameof(EvDbEventRecord.CapturedBy))), 
                CapturedAt = _reader.GetDateTime(_reader.GetOrdinal(nameof(EvDbEventRecord.CapturedAt))), 
                StoredAt = _reader.GetDateTime(_reader.GetOrdinal(nameof(EvDbEventRecord.StoredAt))), 
                Payload = Encoding.UTF8.GetBytes(_reader.GetString(payloadIndex))
            };
            return record;
        }

        EvDbMessageRecord IEvDbRecordParser.ParseMessage()
        {
            var payloadIndex = _reader.GetOrdinal(nameof(EvDbMessageRecord.Payload));
            var record = new EvDbMessageRecord
            {
                Id = _reader.GetGuid(_reader.GetOrdinal(nameof(EvDbMessageRecord.Id))), 
                StreamType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.StreamType))), 
                StreamId = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.StreamId))), 
                Offset = _reader.GetInt64(_reader.GetOrdinal(nameof(EvDbMessageRecord.Offset))), 
                EventType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.EventType))), 
                MessageType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.MessageType))), 
                Channel = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.Channel))), 
                SerializeType = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.SerializeType))), 
                CapturedBy = _reader.GetString(_reader.GetOrdinal(nameof(EvDbMessageRecord.CapturedBy))), 
                CapturedAt = _reader.GetDateTime(_reader.GetOrdinal(nameof(EvDbMessageRecord.CapturedAt))), 
                StoredAt = _reader.GetDateTime(_reader.GetOrdinal(nameof(EvDbMessageRecord.StoredAt))), 
                Payload = _reader.GetFieldValue<byte[]>(payloadIndex)
            };
            return record;
        }
    }

    #endregion //  class RecordParser

    protected override string DatabaseType { get; } = "PostgreSQL";

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
