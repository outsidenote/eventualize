namespace EvDb.Core;

public interface IEvDbStorageScripting
{
    EvDbMigrationQueryTemplates CreateScripts(
                                    EvDbStorageContext context,
                                    StorageFeatures features,
                                    IEnumerable<EvDbShardName> shardNames);
}
