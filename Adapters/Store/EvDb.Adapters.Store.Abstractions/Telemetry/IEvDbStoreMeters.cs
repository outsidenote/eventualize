namespace EvDb.Core.Adapters;

public interface IEvDbStoreMeters
{
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddEvents(int count, EvDbStreamAddress address, string dbType);
    /// <summary>
    /// Events stored into the storage database
    /// </summary>
    void AddMessages(int count, EvDbStreamAddress address, string dbType, EvDbShardName shardName);
}
