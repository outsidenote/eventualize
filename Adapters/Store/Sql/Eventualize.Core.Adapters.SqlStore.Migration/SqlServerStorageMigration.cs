using System.Data.Common;

namespace Eventualize.Core.Adapters.SqlStore;


// TODO: [bnaya 2023-12-10] base it on a shared logic (ADO.NET factory, Db...)
// TODO: [bnaya 2023-12-11] init from configuration
public class SqlServerStorageMigration : RelationalStorageBase, IStorageMigration
{
    public SqlServerStorageMigration(
        Func<DbConnection> factory,
        StorageContext? contextId = null) : base(factory, contextId)
    {
    }

    async Task IStorageMigration.CreateTestEnvironmentAsync()
    {
        await _init;
        if (_contextId.Id == "live")
            throw new ArgumentException("Cannot create a test environment for StorageAdapterContextId='live'");
        string sqlString = SqlOperations.GetCreateEnvironmentQuery(_contextId);
        DbCommand command = _connection.CreateCommand();
        command.CommandText = sqlString;
        await command.ExecuteNonQueryAsync();
    }

    async Task IStorageMigration.DestroyTestEnvironmentAsync()
    {
        await _init;
        if (_contextId.Id == "live")
            throw new ArgumentException("Cannot destroy a test environment for StorageAdapterContextId='live'");
        string sqlString = SqlOperations.GetDestroyEnvironmentQuery(_contextId);
        DbCommand command = _connection.CreateCommand();
        command.CommandText = sqlString;
        await command.ExecuteNonQueryAsync();
    }
}