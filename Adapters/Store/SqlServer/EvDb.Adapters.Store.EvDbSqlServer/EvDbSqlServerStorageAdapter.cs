// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Threading;
using Microsoft.Data.SqlClient.Server;

namespace EvDb.Adapters.Store.SqlServer;

internal class EvDbSqlServerStorageAdapter : EvDbRelationalStorageAdapter
{
    #region Ctor

    public EvDbSqlServerStorageAdapter(ILogger logger,
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
        IEnumerable<SqlDataRecord> dataRecords = ToEventsTvp(records);
        using SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection, (SqlTransaction)transaction);
        insertCommand.CommandType = CommandType.StoredProcedure;
        SqlParameter tvpParam = insertCommand.Parameters.AddWithValue(
                                                            "@Records",
                                                            dataRecords);
        tvpParam.SqlDbType = SqlDbType.Structured;

        int affected = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

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
        var dataRecords = ToOutboxTvp(records);
        query = string.Format(query, shardName);
        SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection, (SqlTransaction)transaction);
        insertCommand.CommandType = CommandType.StoredProcedure;
        SqlParameter tvpParam = insertCommand.Parameters.AddWithValue(
                                                            $"@{shardName}Records",
                                                            dataRecords);
        tvpParam.SqlDbType = SqlDbType.Structured;

        int affected = await insertCommand.ExecuteNonQueryAsync(cancellationToken);

        return affected;
    }

    #endregion //  OnStoreOutboxMessagesAsync

    protected override string DatabaseType { get; } = "sql-server";

    protected override EvDbStreamAdapterQueryTemplates StreamQueries { get; }

    protected override EvDbSnapshotAdapterQueryTemplates SnapshotQueries { get; }

    #region IsOccException

    protected override bool IsOccException(Exception exception)
    {
        bool result = exception is SqlException &&
                      exception.Message.StartsWith("Violation of PRIMARY KEY constraint");
        return result;
    }

    #endregion //  IsOccException

    #region ToEventsTvp

    private static IEnumerable<SqlDataRecord> ToEventsTvp(EvDbEventRecord[] events)
    {
        const int DEFAULT_TEXT_LIMIT = 255; // Replace with actual value if different

        // Define the schema for the TVP with correct sizes
        var metaData = new[]
        {
            new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
            new SqlMetaData("Domain", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData("Partition", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData("StreamId", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData("Offset", SqlDbType.BigInt),
            new SqlMetaData("EventType", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData("SpanId", SqlDbType.VarChar, 16),
            new SqlMetaData("TraceId", SqlDbType.VarChar, 32),
            new SqlMetaData("CapturedBy", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData("CapturedAt", SqlDbType.DateTimeOffset),
            new SqlMetaData("Payload", SqlDbType.VarBinary, 4000)
        };

        // Populate the TVP
        foreach (var message in events)
        {
            var record = new SqlDataRecord(metaData);

            record.SetGuid(0, message.Id);
            record.SetString(1, message.Domain);
            record.SetString(2, message.Partition);
            record.SetString(3, message.StreamId);
            record.SetInt64(4, message.Offset);
            record.SetString(5, message.EventType);

            if (message.SpanId is null)
                record.SetDBNull(6);
            else
                record.SetString(6, message.SpanId);

            if (message.TraceId is null)
                record.SetDBNull(7);
            else
                record.SetString(7, message.TraceId);

            record.SetString(8, message.CapturedBy);
            record.SetDateTimeOffset(9, message.CapturedAt);
            record.SetBytes(10, 0, message.Payload ?? Array.Empty<byte>(), 0, message.Payload?.Length ?? 0);

            yield return record;
        }
    }


    #endregion //  ToEventsTvp

    #region ToOutboxTvp

    private static IEnumerable<SqlDataRecord> ToOutboxTvp(EvDbMessageRecord[] messages)
    {
        const int DEFAULT_TEXT_LIMIT = 255; // Replace with actual value if different

        // Define the schema for the TVP with correct sizes
        var metaData = new[]
        {
        new SqlMetaData("Id", SqlDbType.UniqueIdentifier),
        new SqlMetaData("Domain", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("Partition", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("StreamId", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("Offset", SqlDbType.BigInt),
        new SqlMetaData("EventType", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("Channel", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("MessageType", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("SerializeType", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("SpanId", SqlDbType.VarChar, 16),
        new SqlMetaData("TraceId", SqlDbType.VarChar, 32),
        new SqlMetaData("CapturedBy", SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
        new SqlMetaData("CapturedAt", SqlDbType.DateTimeOffset),
        new SqlMetaData("Payload", SqlDbType.VarBinary, 4000)
    };

        // Populate the TVP
        foreach (var message in messages)
        {
            var record = new SqlDataRecord(metaData);

            record.SetGuid(0, message.Id);
            record.SetString(1, message.Domain);
            record.SetString(2, message.Partition);
            record.SetString(3, message.StreamId);
            record.SetInt64(4, message.Offset);
            record.SetString(5, message.EventType);
            record.SetString(6, message.Channel);
            record.SetString(7, message.MessageType);
            record.SetString(8, message.SerializeType);

            if (message.SpanId is null)
                record.SetDBNull(9);
            else
                record.SetString(9, message.SpanId);

            if (message.TraceId is null)
                record.SetDBNull(10);
            else
                record.SetString(10, message.TraceId);

            record.SetString(11, message.CapturedBy);
            record.SetDateTimeOffset(12, message.CapturedAt);
            record.SetBytes(13, 0, message.Payload ?? Array.Empty<byte>(), 0, message.Payload?.Length ?? 0);

            yield return record;
        }
    }


    #endregion //  ToOutboxTvp
}
