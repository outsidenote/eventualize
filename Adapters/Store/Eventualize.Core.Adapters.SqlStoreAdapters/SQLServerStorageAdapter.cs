using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter;


// TODO: [bnaya 2023-12-10] base it on a shared logic (ADO.NET factory, Db...)
// TODO: [bnaya 2023-12-11] init from configuration
public class SQLServerStorageAdapter : IStorageAdapter
{
    public readonly SqlConnection _connection;
    private readonly StorageAdapterContextId _contextId;
    private readonly Task _init;

    #region ctor

    public SQLServerStorageAdapter(string connectionString, StorageAdapterContextId? contextId = null) : this(new SqlConnection(connectionString), contextId)
    {
    }

    public SQLServerStorageAdapter(SQLServerConnectionData connectionData)
        : this(CreateConnection(connectionData))
    {
    }

    public SQLServerStorageAdapter(SQLServerConnectionData connectionData, StorageAdapterContextId contextId) 
        : this(CreateConnection(connectionData), contextId)
    {
    }

    public SQLServerStorageAdapter(SqlConnection connection, StorageAdapterContextId? contextId = null)
    { 
        _connection = connection;
        _contextId = contextId ?? new StorageAdapterContextId();
        _init = _connection.OpenAsync();
    }

    #endregion // ctor   

    private static SqlConnection CreateConnection(SQLServerConnectionData connectionData)
    {
        SqlConnectionStringBuilder connectionBuilder = new SqlConnectionStringBuilder
        {
            DataSource = connectionData.DataSource,
            UserID = connectionData.UserID,
            Password = connectionData.Password,
            InitialCatalog = connectionData.InitialCatalog,
            TrustServerCertificate = connectionData.TrustServerCertificate,
            MultipleActiveResultSets = connectionData.MultipleActiveResultSets
        };

        var conn = new SqlConnection(connectionBuilder.ConnectionString);
        return conn;
    }

    public async Task<long> GetLastSequenceIdAsync<T>(Aggregate<T> aggregate) where T : notnull, new()
    {
        await _init;
        var command = SQLOperations.SQLOperations.GetLastStoredSnapshotSequenceIdCommand(_connection, _contextId, aggregate);
        if (command == null)
            return default; // TODO: [bnaya 2023-12-07] default of long = 0, is it a valid value? shouldn't we throw an exception?
        var reader = await command.ExecuteReaderAsync();
        reader.Read();
        var sequenceId = reader.GetInt64(0);
        return sequenceId;
    }

    public async Task<StoredSnapshotData<T>?> TryGetSnapshotAsync<T>(string aggregateTypeName, string id) where T : notnull, new()
    {
        await _init;
        var command = SQLOperations.SQLOperations.GetLatestSnapshotCommand<T>(_connection, _contextId, aggregateTypeName, id);
        if (command == null)
            return default;
        var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var jsonData = reader.GetString(0);
        var sequenceId = reader.GetInt64(1);
        var snapshot = JsonSerializer.Deserialize<T>(jsonData);
        if (snapshot == null)
            return default;
        return new StoredSnapshotData<T>(snapshot, sequenceId);
    }

    public async Task<List<EventEntity>> GetAsync(string aggregateTypeName, string id, long startSequenceId)
    {
        await _init;
        List<EventEntity> events = new();
        var command = SQLOperations.SQLOperations.GetStoredEventsCommand(_connection, _contextId, aggregateTypeName, id, startSequenceId);
        if (command == null)
            return events;
        var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var eventType = reader.GetString(0);
            var capturedAt = reader.GetDateTime(1);
            var capturedBy = reader.GetString(2);
            var jsonData = reader.GetString(3);
            var storedAt = reader.GetDateTime(4);
            events.Add(new EventEntity(eventType, capturedAt, capturedBy, jsonData, storedAt));
        }
        return events;
    }

    public async Task CreateTestEnvironment()
    {
        await _init;
        if (_contextId.ContextId == "live")
            throw new ArgumentException("Cannot create a test environment for StorageAdapterContextId='live'");
        string sqlString = SQLOperations.SQLOperations.GetCreateEnvironmentQuery(_contextId);
        SqlCommand command = new SqlCommand(sqlString, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task DestroyTestEnvironment()
    {
        await _init;
        if (_contextId.ContextId == "live")
            throw new ArgumentException("Cannot destroy a test environment for StorageAdapterContextId='live'");
        string sqlString = SQLOperations.SQLOperations.GetDestroyEnvironmentQuery(_contextId);
        SqlCommand command = new SqlCommand(sqlString, _connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<EventEntity>?> SaveAsync<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new()
    {
        var command = SQLOperations.SQLOperations.GetStoreCommand<T>(_connection, _contextId, aggregate, storeSnapshot);
        if (command == null)
            return default;
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException e)
        {
            if (e.Message.Contains("Violation of PRIMARY KEY constraint"))
                throw new OCCException<T>(aggregate);
        }
        return aggregate.PendingEvents ?? default;
    }
}