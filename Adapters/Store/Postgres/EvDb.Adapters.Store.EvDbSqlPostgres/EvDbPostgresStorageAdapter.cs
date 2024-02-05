using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.Postgres;

public class EvDbPostgresStorageAdapter : EvDbRelationalStorageAdapter
{
    public EvDbPostgresStorageAdapter(
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
        throw new NotImplementedException();
    }
}
