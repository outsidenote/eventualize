// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

internal class EvDbSqlServerStorageAdapter : EvDbRelationalStorageAdapter
{
    protected override Task<int> OnStoreStreamEventsAsync(
        DbConnection connection,
        string query,
        EvDbEventRecord[] records,
        DbTransaction transaction)
    {
        return base.OnStoreStreamEventsAsync(connection, query, records, transaction);
    }

    protected override Task<int> OnStoreOutboxMessagesAsync(
        DbConnection connection,
        EvDbShardName shardName,
        string query,
        EvDbMessageRecord[] records,
        DbTransaction transaction)
    {
        var command = new SqlCommand("InsertBatchProcedure", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Add parameter for TVP
        var parameter = new SqlParameter("@MyTable", SqlDbType.Structured)
        {
            TypeName = "MyTableType", // TVP type name in SQL
            Value = items // Directly pass the collection
        };

        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync();
        // return base.OnStoreOutboxMessagesAsync(connection, shardName, query, records, transaction);
    }

    public EvDbSqlServerStorageAdapter(ILogger logger,
        EvDbStorageContext context,
        IEvDbConnectionFactory factory, IEnumerable<IEvDbOutboxTransformer> transformers)
            : base(logger, factory, transformers)
    {
        StreamQueries = QueryTemplatesFactory.CreateStreamQueries(context);
        SnapshotQueries = QueryTemplatesFactory.CreateSnapshotQueries(context);
    }

    protected override string DatabaseType { get; } = "sql-server";

    protected override EvDbStreamAdapterQueryTemplates StreamQueries { get; }

    protected override EvDbSnapshotAdapterQueryTemplates SnapshotQueries { get; }

    protected override bool IsOccException(Exception exception)
    {
        bool result = exception is SqlException &&
                      exception.Message.StartsWith("Violation of PRIMARY KEY constraint");
        return result;
    }
}
