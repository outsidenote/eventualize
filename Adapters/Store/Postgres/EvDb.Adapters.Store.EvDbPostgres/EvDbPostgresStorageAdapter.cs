// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.Common;
using Npgsql;
using Dapper;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace EvDb.Adapters.Store.Postgres;

internal class EvDbPostgresStorageAdapter : EvDbRelationalStorageAdapter
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
        DbTransaction transaction,
        CancellationToken cancellationToken)
    {
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
        List<byte[]> payloads = new ();

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

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection, (NpgsqlTransaction)transaction);
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
        DbTransaction transaction, 
        CancellationToken cancellationToken)
    {
        query = string.Format(query, shardName);

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

        var command = new NpgsqlCommand(query, (NpgsqlConnection)connection, (NpgsqlTransaction)transaction);

        command.Parameters.AddWithValue("@Id", NpgsqlTypes.NpgsqlDbType.Uuid | NpgsqlTypes.NpgsqlDbType.Array, ids);
        command.Parameters.AddWithValue("@Domain", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, domains);
        command.Parameters.AddWithValue("@Partition", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, partitions);
        command.Parameters.AddWithValue("@StreamId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, streamIds);
        command.Parameters.AddWithValue("@Offset", NpgsqlTypes.NpgsqlDbType.Bigint | NpgsqlTypes.NpgsqlDbType.Array, offsets);
        command.Parameters.AddWithValue("@Channel", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@MessageType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@SerializeType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@EventType", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, eventTypes);
        command.Parameters.AddWithValue("@CapturedAt", NpgsqlTypes.NpgsqlDbType.TimestampTz | NpgsqlTypes.NpgsqlDbType.Array, capturedAts);
        command.Parameters.AddWithValue("@CapturedBy", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, capturedBys);
        command.Parameters.AddWithValue("@TraceId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, traceIds);
        command.Parameters.AddWithValue("@SpanId", NpgsqlTypes.NpgsqlDbType.Varchar | NpgsqlTypes.NpgsqlDbType.Array, spanIds);
        command.Parameters.AddWithValue("@Payload", NpgsqlTypes.NpgsqlDbType.Json | NpgsqlTypes.NpgsqlDbType.Array, payloads);

        int affected = await command.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreOutboxMessagesAsync

    protected override string DatabaseType { get; } = "sql-server";

    protected override EvDbStreamAdapterQueryTemplates StreamQueries { get; }

    protected override EvDbSnapshotAdapterQueryTemplates SnapshotQueries { get; }

    #region IsOccException

    protected override bool IsOccException(Exception exception)
    {
        bool result = exception is PostgresException postgresException &&
                 postgresException.SqlState == PostgresErrorCodes.UniqueViolation;
        return result;
    }

    #endregion //  IsOccException
}
