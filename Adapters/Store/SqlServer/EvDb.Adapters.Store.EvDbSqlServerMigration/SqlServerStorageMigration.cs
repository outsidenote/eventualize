using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

internal class SqlServerStorageMigration : EvDbRelationalStorageMigration
{
    public SqlServerStorageMigration(
        ILogger logger,
        EvDbStorageContext context,
        IEvDbConnectionFactory factory)
            : base(logger, factory)
    {
        Queries = QueryTemplatesFactory.Create(context);
    }

    protected override EvDbMigrationQueryTemplates Queries { get; }

}
