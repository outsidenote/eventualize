using System.Data.Common;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertEnvironmentWasCreated
    {
        public static DbCommand GetSqlCommand(TestWorld world)
        {
            var prefix = world.ContextId;
            var queryString = world.TypeOfDb switch
            {
              TypeOfDb.SqlServer =>   
                    $"""
                    SELECT CAST(CASE WHEN OBJECT_ID('{prefix}event', 'U') IS NOT NULL 
                    THEN 1 ELSE 0 END AS BIT) AS [test];
                    """,
              TypeOfDb.Postgress =>
                    $"""
                    SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM information_schema.tables 
                    WHERE table_schema = 'public' AND table_name = '{prefix}event') 
                    THEN true ELSE false END AS BOOLEAN) AS test;
                    """,
                _ => throw new NotImplementedException()
            };
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }
    }
}