using Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;
using Microsoft.Data.SqlClient;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertEnvironmentWasCreated
    {
        public static SqlCommand GetSqlCommand(SQLServerAdapterTestWorld world)
        {
            string prefix = SQLOperations.GetContextIdPrefix(world.ContextId);
            var queryString = $@"
SELECT CAST(CASE WHEN OBJECT_ID('{prefix}event', 'U') IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS [test];
            ";
            return new SqlCommand(queryString, world.StorageAdapter.SQLConnection);
        }
    }
}