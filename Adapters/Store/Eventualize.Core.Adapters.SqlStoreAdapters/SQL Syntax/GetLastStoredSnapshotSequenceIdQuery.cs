namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    // TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
    public static class GetLastStoredSnapshotSequenceIdQuery
    {
        public static string GetSqlString<State>(string contextIdPrefix, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
        {
            // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
            return $@"
SELECT MAX(sequence_id)
FROM {contextIdPrefix}snapshot
WHERE domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'";
        }

    }
}