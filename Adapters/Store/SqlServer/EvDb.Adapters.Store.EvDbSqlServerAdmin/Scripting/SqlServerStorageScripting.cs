// Ignore Spelling: Sql

using EvDb.Core;
// Ignore Spelling: Sql


namespace EvDb.Adapters.Store.SqlServer;

public class SqlServerStorageScripting : IEvDbStorageAdminScripting
{
    public static readonly IEvDbStorageAdminScripting Default = new SqlServerStorageScripting();

    EvDbAdminQueryTemplates IEvDbStorageAdminScripting.CreateScripts(
            EvDbStorageContext context,
            StorageFeatures features,
            IEnumerable<EvDbShardName> shardNames)
    {
        EvDbAdminQueryTemplates scripts = Sctipts.Create(context, features, shardNames);
        return scripts;
    }
}

