// Ignore Spelling: Sql

using EvDb.Core;
using EvDb.Core.Adapters;
using Microsoft.Extensions.Logging;

namespace EvDb.Adapters.Store.SqlServer;

internal class SqlServerStorageMigration : EvDbRelationalStorageMigration
{
    public SqlServerStorageMigration(
        ILogger logger,
        EvDbStorageContext context,
        IEvDbConnectionFactory factory,
        IEnumerable<EvDbShardName> topicShardNames)
            : base(logger, factory)
    {
        Queries = QueryTemplatesFactory.Create(context, topicShardNames);
    }

    protected override EvDbMigrationQueryTemplates Queries { get; }

}
