// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.MongoDB;

public class MongoStorageScripting : IEvDbStorageScripting
{
    public static readonly IEvDbStorageScripting Default = new MongoStorageScripting();

    EvDbMigrationQueryTemplates IEvDbStorageScripting.CreateScripts(
                                                        EvDbStorageContext context,
                                                        StorageFeatures features,
                                                        IEnumerable<EvDbShardName> shardNames)
    {
        throw new NotImplementedException("MongoDB does not support migration scripts.");
    }
}

