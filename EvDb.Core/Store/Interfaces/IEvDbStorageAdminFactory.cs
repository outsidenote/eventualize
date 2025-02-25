// Ignore Spelling: Admin

namespace EvDb.Core;

public interface IEvDbStorageAdminFactory
{
    IEvDbStorageAdmin Create(
            EvDbStorageContext context,
            StorageFeatures features,
            params EvDbShardName[] shardNames);
}
