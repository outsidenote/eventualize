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
        public static void assert(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var sqlCommand = GetStoredEventsSqlCommand(world, aggregate);
            var reader = sqlCommand.ExecuteReader();
            reader.Read();
            var numStoredEvents = reader.GetInt32(0);
            Assert.AreEqual(aggregate.PendingEvents.Count, numStoredEvents);

            if (!isSnapshotStored)
                return;

            sqlCommand = GetStoredSnapshotSqlCommand(world,aggregate);
            reader = sqlCommand.ExecuteReader();
            reader.Read();
            var snapshotSequenceId = reader.GetInt64(0);
            Assert.AreEqual(aggregate.LastStoredSequenceId+aggregate.PendingEvents.Count,snapshotSequenceId);


        }
        private static SqlCommand GetStoredEventsSqlCommand(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
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

        private static SqlCommand GetStoredSnapshotSqlCommand(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
        {

            string prefix = SQLOperations.GetContextIdPrefix(world.ContextId);
            var queryString = $@"
SELECT sequence_id
FROM {prefix}snapshot
WHERE
    domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'
            ";
            return new SqlCommand(queryString, world.StorageAdapter.SQLConnection);

        }

    }
}