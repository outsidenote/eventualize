namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
// TODO: [bnaya 2023-12-10] use parameters (not concatenation)
internal static class GetLatestSnapshotQuery
{
    public static string GetSqlString(StorageContext contextIdPrefix, string aggregateTypeName, string id)
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $"""
            SELECT json_data, sequence_id
            FROM {contextIdPrefix}snapshot
            WHERE domain = 'default'
                AND aggregate_type = '{aggregateTypeName}'
                AND aggregate_id = '{id}'
            ORDER BY sequence_id DESC
            OFFSET 0 ROWS FETCH FIRST 1 ROWS ONLY;
            """;
    }

}