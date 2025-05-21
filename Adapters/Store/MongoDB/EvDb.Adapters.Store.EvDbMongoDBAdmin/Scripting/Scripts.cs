using EvDb.Core;

namespace EvDb.Adapters.Store.MongoDB;

internal static class Scripts
{
    public static EvDbMigrationQueryTemplates Create(
                            EvDbStorageContext storageContext,
                            StorageFeatures features,
                            IEnumerable<EvDbShardName> outboxShardNames)
    {
        throw new NotImplementedException("MongoDB does not support migration scripts.");
    }
}
