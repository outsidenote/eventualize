// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using static EvDb.Core.Adapters.Internals.EvDbStoreNames.Projection;

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
        //DbTransaction transaction,
        CancellationToken cancellationToken)
    {
        IEnumerable<SqlDataRecord> dataRecords = ToEventsTvp(records);
        using SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection);//, (SqlTransaction)transaction);
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
        //DbTransaction transaction, 
        CancellationToken cancellationToken)
    {
        var dataRecords = ToOutboxTvp(records);
        query = string.Format(query, shardName);
        SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection); //, (SqlTransaction)transaction);
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
            new SqlMetaData(Event.Id, SqlDbType.UniqueIdentifier),
            new SqlMetaData(Event.Domain, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.Partition, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.StreamId, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.Offset, SqlDbType.BigInt),
            new SqlMetaData(Event.EventType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.CapturedBy, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.CapturedAt, SqlDbType.DateTimeOffset),
            new SqlMetaData(Event.TelemetryContext, SqlDbType.VarBinary, 2000),
            new SqlMetaData(Event.Payload, SqlDbType.VarBinary, 4000)
        };

        var otelContext = Activity.Current?.SerializeTelemetryContext();

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

            record.SetString(6, message.CapturedBy);
            record.SetDateTimeOffset(7, message.CapturedAt);

            if (otelContext is null)
                record.SetDBNull(8);
            else
                record.SetBytes(8, 0, otelContext, 0, otelContext?.Length ?? 0);
            record.SetBytes(9, 0, message.Payload ?? Array.Empty<byte>(), 0, message.Payload?.Length ?? 0);

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
            new SqlMetaData(Message.Id, SqlDbType.UniqueIdentifier),
            new SqlMetaData(Message.Domain, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.Partition, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.StreamId, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.Offset, SqlDbType.BigInt),
            new SqlMetaData(Message.EventType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.Channel, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.MessageType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.SerializeType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.CapturedBy, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Message.CapturedAt, SqlDbType.DateTimeOffset),
            new SqlMetaData(Message.TelemetryContext, SqlDbType.VarBinary, 2000),
            new SqlMetaData(Message.Payload, SqlDbType.VarBinary, 4000)
        };

        var otelContext = Activity.Current?.SerializeTelemetryContext();

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


            record.SetString(9, message.CapturedBy);
            record.SetDateTimeOffset(10, message.CapturedAt);

            if (otelContext is null)
                record.SetDBNull(11);
            else
                record.SetBytes(11, 0, otelContext, 0, otelContext.Length);
            record.SetBytes(12, 0, message.Payload ?? Array.Empty<byte>(), 0, message.Payload?.Length ?? 0);

            yield return record;
        }
    }


    #endregion //  ToOutboxTvp
}
