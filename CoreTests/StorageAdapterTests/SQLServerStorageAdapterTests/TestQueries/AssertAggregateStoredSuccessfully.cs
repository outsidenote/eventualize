using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;
using CoreTests.AggregateTypeTests;
using Microsoft.Data.SqlClient;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertAggregateStoredSuccessfully
    {
        public static void assert(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
        {
            var sqlCommand = GetSqlCommand(world, aggregate);
            var reader = sqlCommand.ExecuteReader();
            reader.Read();
            var numStoredEvents = reader.GetInt32(0);
            Assert.AreEqual(3, numStoredEvents);
        }
        private static SqlCommand GetSqlCommand(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
        {

            string prefix = SQLOperations.GetContextIdPrefix(world.ContextId);
            var queryString = $@"
SELECT COUNT(*)
FROM {prefix}event
WHERE
    domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'
            ";
            return new SqlCommand(queryString, world.StorageAdapter.SQLConnection);

        }

    }
}