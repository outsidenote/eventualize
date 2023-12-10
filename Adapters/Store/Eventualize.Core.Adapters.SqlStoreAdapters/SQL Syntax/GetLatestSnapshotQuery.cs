using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;

// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
public static class GetLatestSnapshotQuery
{
    public static string GetSqlString<State>(string contextIdPrefix, string aggregateTypeName, string id) where State : notnull, new()
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $@"
SELECT json_data, sequence_id
FROM {contextIdPrefix}snapshot
WHERE domain = 'default'
    AND aggregate_type = '{aggregateTypeName}'
    AND aggregate_id = '{id}'
ORDER BY sequence_id DESC
OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;";
    }

}