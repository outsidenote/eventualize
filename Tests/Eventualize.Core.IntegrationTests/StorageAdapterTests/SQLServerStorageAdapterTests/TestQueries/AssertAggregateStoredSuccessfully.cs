using Eventualize.Core;
using Eventualize.Core.Tests;
using System.Data.Common;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests.TestQueries
{
    public static class AssertAggregateStoredSuccessfully
    {
        public static async Task assert(TestWorld world, EventualizeAggregate<TestState> aggregate, bool isSnapshotStored)
        {
            var sqlCommand = GetStoredEventsSqlCommand(world, aggregate);
            var reader = sqlCommand.ExecuteReader();
            reader.Read();
            var numStoredEvents = reader.GetInt32(0);
            Assert.Equal(aggregate.PendingEvents.Count, numStoredEvents);

            if (!isSnapshotStored)
                return;

            var snp = await  world.StorageAdapter.TryGetSnapshotAsync<TestState>(new AggregateParameter(aggregate.Id, aggregate.Type));
            var snapshotSequenceId = snp.SnapshotSequenceId;// reader.GetInt64(0);
            Assert.Equal(aggregate.LastStoredSequenceId + aggregate.PendingEvents.Count, snapshotSequenceId);
        }

        private static DbCommand GetStoredEventsSqlCommand(TestWorld world, EventualizeAggregate<TestState> aggregate)
        {

            var prefix = world.ContextId;
            var queryString = $@"
SELECT COUNT(*)
FROM {prefix}event
WHERE
    domain = 'default'
    AND aggregate_type = '{aggregate.Type}'
    AND aggregate_id = '{aggregate.Id}'
            ";
            var command = world.Connection.CreateCommand();
            command.CommandText = queryString;
            return command;
        }

    //        private static DbCommand GetStoredSnapshotSqlCommand(TestWorld world, EventualizeAggregate<TestState> aggregate)
    //        {

    //            var prefix = world.ContextId;
    //            var queryString = $@"
    //SELECT sequence_id
    //FROM {prefix}snapshot
    //WHERE
    //    domain = 'default'
    //    AND aggregate_type = '{aggregate.Type}'
    //    AND aggregate_id = '{aggregate.Id}'
    //            ";
    //            var command = world.Connection.CreateCommand();
    //            command.CommandText = queryString;
    //            return command;
    //        }

    }
}