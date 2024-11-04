// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

internal class EvDbSqlServerStorageAdapter : EvDbRelationalStorageAdapter
{
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
