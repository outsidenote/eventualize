// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.Postgres;

public class PostgresStorageScripting : IEvDbStorageAdminScripting
{
    public static readonly IEvDbStorageAdminScripting Default = new PostgresStorageScripting();

    EvDbAdminQueryTemplates IEvDbStorageAdminScripting.CreateScripts(
                                                        EvDbStorageContext context,
                                                        StorageFeatures features,
                                                        IEnumerable<EvDbShardName> shardNames)
    {
        EvDbAdminQueryTemplates scripts = Scripts.Create(context, features, shardNames);
        return scripts;
    }
}

