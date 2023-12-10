using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class GetStoredEventsQuery
    {
        public static string GetSqlString(string contextIdPrefix, string aggregateTypeName, string id, long startSequenceId)
        {
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
}