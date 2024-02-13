using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace EvDb.Adapters.Store.SqlServer;

internal class EvDbSqlServerStorageAdapter : EvDbRelationalStorageAdapter
{
    public EvDbSqlServerStorageAdapter(
        ILogger logger,
        EvDbStorageContext context,
        IEvDbConnectionFactory factory) 
            : base(logger, factory)
    {
        Queries = QueryTemplatesFactory.Create(context);
    }

    protected override EvDbAdapterQueryTemplates Queries { get; } 

    protected override bool IsOccException(Exception exception)
    {
        bool result = exception is SqlException && 
                      exception.Message.StartsWith("Violation of PRIMARY KEY constraint");
        return result;
    }
}
