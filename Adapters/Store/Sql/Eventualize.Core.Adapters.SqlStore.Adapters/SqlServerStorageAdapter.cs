using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Eventualize.Core.Adapters.SqlStore;

// TODO: [bnaya 2023-12-10] base it on a shared logic (ADO.NET factory, Db...)
// TODO: [bnaya 2023-12-11] init from configuration
public sealed class SqlServerStorageAdapter : RelationalStorageAdapterBase
{
    public SqlServerStorageAdapter(Func<DbConnection> factory, StorageAdapterContextId? contextId = null) : base(factory, contextId)
    {
    }

    protected override DbCommand GetLastStoredSnapshotSequenceIdCommand<TState>(Aggregate<TState> aggregate)
    {
        return SqlOperations.GetLastStoredSnapshotSequenceIdCommand(_connection, _contextId, aggregate);
    }

    protected override DbCommand GetLatestSnapshotCommand(string aggregateTypeName, string id)
    {
        return SqlOperations.GetLatestSnapshotCommand(_connection, _contextId, aggregateTypeName, id);
    }

    protected override DbCommand GetStoreCommand<TState>(
        Aggregate<TState> aggregate,
        bool isSnapshotStored)
    {
        return SqlOperations.GetStoreCommand(_connection, _contextId, aggregate, isSnapshotStored);
    }

    protected override DbCommand GetStoredEventsCommand(
        string aggregateTypeName,
        string id,
        long startSequenceId)
    {
        return SqlOperations.GetStoredEventsCommand(_connection, _contextId, aggregateTypeName, id, startSequenceId);
    }
}