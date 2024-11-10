namespace EvDb.Core.Adapters;

internal interface IEvDbStoreMeters
{
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddEvents(int count, IEvDbStreamStoreData streamStore, string dbType);
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddMessages(int count, IEvDbStreamStoreData streamStore, string dbType, EvDbShardName shardName);
}
