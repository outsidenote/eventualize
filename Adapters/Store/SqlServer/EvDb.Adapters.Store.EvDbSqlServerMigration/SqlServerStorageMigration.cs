﻿// Ignore Spelling: Sql

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
        StorageFeatures features,
        IEnumerable<EvDbShardName> shardNames)
            : base(logger, factory)
    {
        Queries = QueryProvider.Create(context, features, shardNames);
    }

    protected override EvDbMigrationQueryTemplates Queries { get; }

}
