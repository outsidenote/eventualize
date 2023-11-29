using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class StoreQuery
    {
        public static string? GetSqlString<State>(string contextIdPrefix, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
        {
            var events = aggregate.PendingEvents;
            if (events.Count == 0)
                return null;
            var sqlString = $@"
INSERT INTO {contextIdPrefix}event (domain, aggregate_type, aggregate_id,sequence_id,captured_at, event_type, captured_by,json_data)
VALUES
";
            var valuesString = string.Join(",", aggregate.PendingEvents.Select((e, index) => $@"
('default', '{aggregate.AggregateType.Name}', '{aggregate.Id}',{aggregate.LastStoredSequenceId + index + 1},'{e.CapturedAt.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}','{e.EventType}','{e.CapturedBy}','{e.JsonData}')"));
            sqlString += valuesString + ";";

            return sqlString;

        }

    }
}