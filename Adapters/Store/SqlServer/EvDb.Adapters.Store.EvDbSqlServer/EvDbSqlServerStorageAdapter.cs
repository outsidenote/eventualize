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
        using SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection);
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
        SqlCommand insertCommand = new SqlCommand(query, (SqlConnection)connection);
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
            new SqlMetaData(Event.StreamType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.StreamId, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.Offset, SqlDbType.BigInt),
            new SqlMetaData(Event.EventType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.CapturedBy, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
            new SqlMetaData(Event.CapturedAt, SqlDbType.DateTimeOffset),
            new SqlMetaData(Event.TelemetryContext, SqlDbType.VarBinary, 2000),
            new SqlMetaData(Event.Payload, SqlDbType.VarBinary, 4000)
        };

        EvDbTelemetryContextName otelContext = Activity.Current?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty;

        // Populate the TVP
        foreach (var ev in events)
        {
            var record = new SqlDataRecord(metaData);

            record.SetGuid(0, ev.Id);
            record.SetString(1, ev.StreamType);
            record.SetString(2, ev.StreamId);
            record.SetInt64(3, ev.Offset);
            record.SetString(4, ev.EventType);

            record.SetString(5, ev.CapturedBy);
            record.SetDateTimeOffset(6, ev.CapturedAt);

            if (otelContext.Length == 0)
                record.SetDBNull(7);
            else
            {
                IEvDbPayloadRawData otelRaw = otelContext;
                record.SetBytes(7, 0, otelRaw.RawValue, 0, otelContext.Length);
            }
            IEvDbPayloadRawData payloadRaw = ev.Payload;
            record.SetBytes(8, 0, payloadRaw.RawValue, 0, ev.Payload.Length);

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
            new SqlMetaData(Message.StreamType, SqlDbType.NVarChar, DEFAULT_TEXT_LIMIT),
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

        var otelContext = Activity.Current?.SerializeTelemetryContext() ?? EvDbTelemetryContextName.Empty;

        // Populate the TVP
        foreach (var message in messages)
        {
            var record = new SqlDataRecord(metaData);

            record.SetGuid(0, message.Id);
            record.SetString(1, message.StreamType);
            record.SetString(2, message.StreamId);
            record.SetInt64(3, message.Offset);
            record.SetString(4, message.EventType);
            record.SetString(5, message.Channel);
            record.SetString(6, message.MessageType);
            record.SetString(7, message.SerializeType);


            record.SetString(8, message.CapturedBy);
            record.SetDateTimeOffset(9, message.CapturedAt);

            if (otelContext == EvDbTelemetryContextName.Empty)
                record.SetDBNull(10);
            else
            {
                IEvDbPayloadRawData otelRaw = otelContext;
                record.SetBytes(10, 0, otelRaw.RawValue, 0, otelContext.Length);
            }
            IEvDbPayloadRawData payloadRaw = message.Payload;
            record.SetBytes(11, 0, payloadRaw.RawValue, 0, message.Payload.Length);

            yield return record;
        }
    }


    #endregion //  ToOutboxTvp
}
