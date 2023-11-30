using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class StoreQuery
    {
        public static string? GetStorePendingEventsSqlString<State>(string contextIdPrefix, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
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

//             var occFutureConstraintString = $@"
// WHERE {aggregate.LastStoredSequenceId} = (
//             SELECT MAX(sequence_id)
//             FROM {contextIdPrefix}event
//             WHERE domain = 'example_domain'
//                 AND aggregate_type = 'example_aggregate_type'
//                 AND aggregate_id = 'example_aggregate_id'
//         )";
//             sqlString += occFutureConstraintString + ';';

            return sqlString;

        }

        public static string? GetStoreSnapshotSqlString<State>(string contextIdPrefix, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
        {
            var snapshotData = JsonSerializer.Serialize(aggregate.State, typeof(State));
            var sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
            var sqlString = $@"
INSERT INTO {contextIdPrefix}snapshot (domain, aggregate_type, aggregate_id,sequence_id,json_data)
VALUES ('default', '{aggregate.AggregateType.Name}', '{aggregate.Id}',{sequenceId},'{snapshotData}');";
            return sqlString;
        }

    }
}