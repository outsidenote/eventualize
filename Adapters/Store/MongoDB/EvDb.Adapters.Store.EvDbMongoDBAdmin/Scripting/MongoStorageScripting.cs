// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.Postgres;

public class MongoStorageScripting : IEvDbStorageScripting
{
    public static readonly IEvDbStorageScripting Default = new MongoStorageScripting();

    EvDbMigrationQueryTemplates IEvDbStorageScripting.CreateScripts(
                                                        EvDbStorageContext context,
                                                        StorageFeatures features,
                                                        IEnumerable<EvDbShardName> shardNames)
    {
        EvDbMigrationQueryTemplates scripts = Scripts.Create(context, features, shardNames);
        return scripts;
    }
}

