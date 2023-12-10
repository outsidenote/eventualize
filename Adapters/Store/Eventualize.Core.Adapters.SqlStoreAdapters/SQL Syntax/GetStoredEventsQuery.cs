using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;

// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
public static class GetStoredEventsQuery
{
    public static string GetSqlString(string contextIdPrefix, string aggregateTypeName, string id, long startSequenceId)
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $@"
SELECT
    event_type,
    captured_at,
    captured_by,
    json_data,
    stored_at
FROM {contextIdPrefix}event
WHERE domain = 'default'
    AND aggregate_type = '{aggregateTypeName}'
    AND aggregate_id = '{id}'
    and sequence_id >= {startSequenceId};";
    }

}