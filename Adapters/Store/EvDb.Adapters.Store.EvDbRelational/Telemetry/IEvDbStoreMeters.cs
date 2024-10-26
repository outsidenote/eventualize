using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.Metrics;
using VogenTableName;

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
    void AddMessages(int count, IEvDbStreamStoreData streamStore, string dbType, EvDbTableName tableName);
}
