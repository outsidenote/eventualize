// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.SqlServer;

public class SqlServerStorageScripting : IEvDbStorageScripting
{
    public static readonly IEvDbStorageScripting Default = new SqlServerStorageScripting();

    EvDbMigrationQueryTemplates IEvDbStorageScripting.CreateScripts(
            EvDbStorageContext context,
            StorageFeatures features,
            IEnumerable<EvDbShardName> shardNames)
    {
        EvDbMigrationQueryTemplates scripts = Sctipts.Create(context, features, shardNames);
        return scripts;
    }
}

