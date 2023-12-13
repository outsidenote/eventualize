namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] migration should move to a different project 
// TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
internal static class DestroyEnvironmentQuery
{
    public static string GetSqlString(StorageContext contextIdPrefix)
    {
        // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
        return $"""
            DROP TABLE {contextIdPrefix}event;
            DROP TABLE {contextIdPrefix}snapshot;            
            """;
    }
}