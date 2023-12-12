using System.Data.Common;

namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] review responsibility, abstraction and implementation (testcontextid_ shouldn't be part of this class in this place)
// TODO: [bnaya 2023-12-10] consider builder patten to setup the context
// TODO: [bnaya 2023-12-10] consider to put the context into the DI
internal static class SqlOperations
{
    public static DbCommand GetStoreCommand<State>(
        DbConnection connection,
        StorageAdapterContextId contextId,
        Aggregate<State> aggregate,
        bool isSnapshotStored) where State : notnull, new()
    {
        string? sqlString = StoreQuery.GetStorePendingEventsSqlString(contextId, aggregate);
        if (string.IsNullOrEmpty(sqlString))
            throw new ArgumentException("sql-query");
        if (isSnapshotStored)
            sqlString += StoreQuery.GetStoreSnapshotSqlString(contextId, aggregate);
        var cmd = connection.CreateCommand();
        cmd.CommandText = sqlString;
        return cmd;
    }

    public static DbCommand GetLastStoredSnapshotSequenceIdCommand<State>(
        DbConnection connection,
        StorageAdapterContextId contextId,
        Aggregate<State> aggregate) where State : notnull, new()
    {
        var sqlString = GetLastStoredSnapshotSequenceIdQuery.GetSqlString(contextId, aggregate);
        if (string.IsNullOrEmpty(sqlString))
            throw new ArgumentException("sql-query");
        var cmd = connection.CreateCommand();
        cmd.CommandText = sqlString;
        return cmd;
    }

    public static DbCommand GetLatestSnapshotCommand(
        DbConnection connection,
        StorageAdapterContextId contextId,
        string aggregateTypeName,
        string id)
    {
        var sqlString = GetLatestSnapshotQuery.GetSqlString(contextId, aggregateTypeName, id);
        if (string.IsNullOrEmpty(sqlString))
            throw new ArgumentException("sql-query");
        var cmd = connection.CreateCommand();
        cmd.CommandText = sqlString;
        return cmd;
    }

    public static DbCommand GetStoredEventsCommand(
        DbConnection connection,
        StorageAdapterContextId contextId,
        string aggregateTypeName,
        string id,
        long startSequenceId)
    {
        var sqlString = GetStoredEventsQuery.GetSqlString(contextId, aggregateTypeName, id, startSequenceId);
        if (string.IsNullOrEmpty(sqlString))
            throw new ArgumentException("sql-query");
        var cmd = connection.CreateCommand();
        cmd.CommandText = sqlString;
        return cmd;
    }

}