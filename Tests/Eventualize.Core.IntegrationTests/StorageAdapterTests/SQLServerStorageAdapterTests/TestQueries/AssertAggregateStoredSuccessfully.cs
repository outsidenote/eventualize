using Eventualize.Core;
using Eventualize.Core.Tests;
using Microsoft.Data.SqlClient;
using System.Data.Common;

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
            Assert.Equal(aggregate.PendingEvents.Count, numStoredEvents);

            if (!isSnapshotStored)
                return;

            sqlCommand = GetStoredSnapshotSqlCommand(world, aggregate);
            reader = sqlCommand.ExecuteReader();
            reader.Read();
            var snapshotSequenceId = reader.GetInt64(0);
            Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, snapshotSequenceId);
        }

        private static DbCommand GetStoredEventsSqlCommand(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
        {

            var prefix = world.ContextId;
            var queryString = $@"
SELECT COUNT(*)
FROM {prefix}event
WHERE
    domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }

        private static DbCommand GetStoredSnapshotSqlCommand(SQLServerAdapterTestWorld world, Aggregate<TestState> aggregate)
        {

            var prefix = world.ContextId;
            var queryString = $@"
SELECT sequence_id
FROM {prefix}snapshot
WHERE
    domain = 'default'
    AND aggregate_type = '{aggregate.AggregateType.Name}'
    AND aggregate_id = '{aggregate.Id}'
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }

    }
}