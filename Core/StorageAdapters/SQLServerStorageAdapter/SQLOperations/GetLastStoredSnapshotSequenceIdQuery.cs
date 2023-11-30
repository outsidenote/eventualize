using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class GetLastStoredSnapshotSequenceIdQuery
    {
        public static string GetSqlString<State>(string contextIdPrefix, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
        {
            return $@"
SELECT MAX(sequence_id)
FROM {contextIdPrefix}snapshot
WHERE domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'";
        }

    }
}