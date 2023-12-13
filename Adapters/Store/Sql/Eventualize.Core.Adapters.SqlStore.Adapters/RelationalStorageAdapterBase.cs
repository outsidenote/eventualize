using Microsoft.Data.SqlClient;
using System.Data.Common;
using System.Text.Json;

namespace Eventualize.Core.Adapters.SqlStore;

public abstract class RelationalStorageAdapterBase : RelationalStorageBase, IStorageAdapter
{
    public RelationalStorageAdapterBase(
        Func<DbConnection> factory,
        StorageContext? contextId = null) : base(factory, contextId)
    {
    }

    // TODO: [bnaya 2023-12-12] remove Aggregate<T> aggregate when using params
    protected abstract DbCommand GetLastStoredSnapshotSequenceIdCommand<TState>(Aggregate<TState> aggregate) where TState : notnull, new();

    protected abstract DbCommand GetLatestSnapshotCommand(string aggregateTypeName, string id);

    // TODO: [bnaya 2023-12-12] remove Aggregate<T> aggregate when using params
    protected abstract DbCommand GetStoredEventsCommand(
         string aggregateTypeName,
         string id,
         long startSequenceId);

    // TODO: [bnaya 2023-12-12] remove Aggregate<T> aggregate when using params
    protected abstract DbCommand GetStoreCommand<TState>(Aggregate<TState> aggregate, bool isSnapshotStored) where TState : notnull, new();

    async Task<long> IStorageAdapter.GetLastSequenceIdAsync<T>(Aggregate<T> aggregate)
    {
        await _init;
        DbCommand command = GetLastStoredSnapshotSequenceIdCommand(aggregate);
        DbDataReader reader = await command.ExecuteReaderAsync();
        reader.Read();
        var sequenceId = reader.GetInt64(0);
        return sequenceId;
    }

    async Task<StoredSnapshotData<T>?> IStorageAdapter.TryGetSnapshotAsync<T>(
        string aggregateTypeName,
        string id)
    {
        await _init;
        DbCommand command = GetLatestSnapshotCommand(aggregateTypeName, id);
        DbDataReader reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var jsonData = reader.GetString(0);
        var sequenceId = reader.GetInt64(1);
        var snapshot = JsonSerializer.Deserialize<T>(jsonData);
        if (snapshot == null)
            return default;
        return new StoredSnapshotData<T>(snapshot, sequenceId);
    }

    async Task<List<EventEntity>> IStorageAdapter.GetAsync(
                            string aggregateTypeName,
                            string id,
                            long startSequenceId)
    {
        await _init;
        List<EventEntity> events = new();
        DbCommand command = GetStoredEventsCommand(aggregateTypeName, id, startSequenceId);
        DbDataReader reader = await command.ExecuteReaderAsync();
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

    async Task<List<EventEntity>> IStorageAdapter.SaveAsync<T>(Aggregate<T> aggregate, bool storeSnapshot)
    {
        DbCommand command = GetStoreCommand(aggregate, storeSnapshot);
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
        return aggregate.PendingEvents;
    }
}
