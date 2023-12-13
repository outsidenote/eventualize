using System.Data.Common;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertEnvironmentWasCreated
    {
        public static DbCommand GetSqlCommand(SQLServerAdapterTestWorld world)
        {
            var prefix = world.ContextId;
            var queryString = $@"
SELECT CAST(CASE WHEN OBJECT_ID('{prefix}event', 'U') IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS [test];
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }
    }
}