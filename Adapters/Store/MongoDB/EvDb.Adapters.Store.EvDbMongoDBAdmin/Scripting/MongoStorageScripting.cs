// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.MongoDB;

public class MongoStorageScripting : IEvDbStorageAdminScripting
{
    public static readonly IEvDbStorageAdminScripting Default = new MongoStorageScripting();

    EvDbAdminQueryTemplates IEvDbStorageAdminScripting.CreateScripts(
                                                        EvDbStorageContext context,
                                                        StorageFeatures features,
                                                        IEnumerable<EvDbShardName> shardNames)
    {
        throw new NotImplementedException("MongoDB does not support migration scripts.");
    }
}

