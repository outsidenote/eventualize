using System.Text.Json;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;

// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
public static class StoreQuery
{
    public static string? GetStorePendingEventsSqlString<State>(string contextIdPrefix, Aggregate<State> aggregate) where State : notnull, new()
    {
        var events = aggregate.PendingEvents;
        if (events.Count == 0)
            return null;
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        var sqlString = $@"
INSERT INTO {contextIdPrefix}event (domain, aggregate_type, aggregate_id,sequence_id,captured_at, event_type, captured_by,json_data)
VALUES
";
        var valuesString = string.Join(",", aggregate.PendingEvents.Select((e, index) => $@"
('default', '{aggregate.AggregateType.Name}', '{aggregate.Id}',{aggregate.LastStoredSequenceId + index + 1},'{e.CapturedAt.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}','{e.EventType}','{e.CapturedBy}','{e.JsonData}')"));
        sqlString += valuesString + ";";

        return sqlString;

    }

    public static string? GetStoreSnapshotSqlString<State>(string contextIdPrefix, Aggregate<State> aggregate) where State : notnull, new()
    {
        var snapshotData = JsonSerializer.Serialize(aggregate.State, typeof(State));
        var sequenceId = aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count;
        var sqlString = $@"
INSERT INTO {contextIdPrefix}snapshot (domain, aggregate_type, aggregate_id,sequence_id,json_data)
VALUES ('default', '{aggregate.AggregateType.Name}', '{aggregate.Id}',{sequenceId},'{snapshotData}');";
        return sqlString;
    }

}