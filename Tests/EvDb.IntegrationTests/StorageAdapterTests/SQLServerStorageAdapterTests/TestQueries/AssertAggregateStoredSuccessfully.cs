using EvDb.Core;
using EvDb.Core.Tests;
using System.Data.Common;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertAggregateStoredSuccessfully
    {
        public static void assert(SQLServerAdapterTestWorld world, EvDbAggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var sqlCommand = GetStoredEventsSqlCommand(world, aggregate);
            var reader = sqlCommand.ExecuteReader();
            reader.Read();
            var numStoredEvents = reader.GetInt32(0);
            Assert.Equal(aggregate.EventsCount, numStoredEvents);

            if (!isSnapshotStored)
                return;

            sqlCommand = GetStoredSnapshotSqlCommand(world, aggregate);
            reader = sqlCommand.ExecuteReader();
            reader.Read();
            var snapshotOffset = reader.GetInt64(0);
            Assert.Equal(aggregate.LastStoredOffset + aggregate.EventsCount, snapshotOffset);
        }

        private static DbCommand GetStoredEventsSqlCommand(SQLServerAdapterTestWorld world, EvDbAggregate<TestState> aggregate)
        {

            var prefix = world.ContextId;
            var queryString = $@"
SELECT COUNT(*)
FROM {prefix}event
WHERE
    domain = 'default'
    AND stream_type = '{aggregate.StreamId.Partition}'
    AND stream_id = '{aggregate.StreamId.StreamId}'
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }

        private static DbCommand GetStoredSnapshotSqlCommand(SQLServerAdapterTestWorld world, EvDbAggregate<TestState> aggregate)
        {

            var prefix = world.ContextId;
            var queryString = $@"
SELECT offset
FROM {prefix}snapshot
WHERE
    domain = 'default'
    AND stream_type = '{aggregate.StreamId.Partition}'
    AND stream_id = '{aggregate.StreamId.StreamId}'
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }

    }
}