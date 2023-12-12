namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
// TODO: [bnaya 2023-12-10] use parameters (not concatenation)
internal static class GetLastStoredSnapshotSequenceIdQuery
{
    public static string GetSqlString<State>(StorageAdapterContextId contextIdPrefix, Aggregate<State> aggregate) where State : notnull, new()
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $"""
                SELECT MAX(sequence_id)
                    FROM {contextIdPrefix}snapshot
                    WHERE domain = 'default'
                        AND aggregate_type = '{aggregate.AggregateType.Name}'
                        AND aggregate_id = '{aggregate.Id}'
                """;
    }
}