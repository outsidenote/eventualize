// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.Postgres;

public class PostgresStorageScripting : IEvDbStorageScripting
{
    public static readonly IEvDbStorageScripting Default = new PostgresStorageScripting();

    EvDbMigrationQueryTemplates IEvDbStorageScripting.CreateScripts(
                                                        EvDbStorageContext context,
                                                        StorageFeatures features,
                                                        IEnumerable<EvDbShardName> shardNames)
    {
        EvDbMigrationQueryTemplates scripts = Scripts.Create(context, features, shardNames);
        return scripts;
    }
}

