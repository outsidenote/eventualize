namespace EvDb.Core;

public interface IEvDbStorageAdminScripting
{
    EvDbAdminQueryTemplates CreateScripts(
                                    EvDbStorageContext context,
                                    StorageFeatures features,
                                    IEnumerable<EvDbShardName> shardNames);
}
