using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;
using Microsoft.Data.SqlClient;
using Core.StorageAdapters;
using Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;
using System.Text.Json;

namespace Core.StorageAdapters.SQLServerStorageAdapter
{
    public class SQLServerStorageAdapter : IStorageAdapter
    {
        public readonly SqlConnection SQLConnection;
        private StorageAdapterContextId ContextId;

        public SQLServerStorageAdapter(string connectionString)
        {
            SQLConnection = new SqlConnection(connectionString);
            ContextId = new StorageAdapterContextId();
        }

        public SQLServerStorageAdapter(string connectionString, StorageAdapterContextId contextId)
        {
            SQLConnection = new SqlConnection(connectionString);
            ContextId = contextId;
        }

        public SQLServerStorageAdapter(SQLServerConnectionData connectionData)
        {
            SqlConnectionStringBuilder connectionBuilder = new SqlConnectionStringBuilder
            {
                DataSource = connectionData.DataSource,
                UserID = connectionData.UserID,
                Password = connectionData.Password,
                InitialCatalog = connectionData.InitialCatalog
            };

            SQLConnection = new SqlConnection(connectionBuilder.ConnectionString);
            ContextId = new StorageAdapterContextId();
        }
        public SQLServerStorageAdapter(SQLServerConnectionData connectionData, StorageAdapterContextId contextId)
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

            SQLConnection = new SqlConnection(connectionBuilder.ConnectionString);
            ContextId = contextId;
        }
        public Task<long> GetLastStoredSequenceId<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            var command = SQLOperations.SQLOperations.GetLastStoredSnapshotSequenceIdCommand(SQLConnection, ContextId, aggregate);
            if (command == null)
                return Task.FromResult(default(long));
            var reader = command.ExecuteReader();
            reader.Read();
            var sequenceId = reader.GetInt64(0);
            return Task.FromResult(sequenceId);
        }

        public Task<StoredSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new()
        {
            var command = SQLOperations.SQLOperations.GetLatestSnapshotCommand<T>(SQLConnection, ContextId, aggregateTypeName, id);
            if (command == null)
                return Task.FromResult(default(StoredSnapshotData<T>));
            var reader = command.ExecuteReader();
            reader.Read();
            var jsonData = reader.GetString(0);
            var sequenceId = reader.GetInt64(1);
            var snapshot = JsonSerializer.Deserialize<T>(jsonData);
            if (snapshot == null)
                return Task.FromResult(default(StoredSnapshotData<T>));
            return Task.FromResult(new StoredSnapshotData<T>(snapshot, sequenceId) ?? default(StoredSnapshotData<T>));
        }

        public Task<List<Event.Event>> GetStoredEvents(string aggregateTypeName, string id, long startSequenceId)
        {
            List<Event.Event> events = new();
            var command = SQLOperations.SQLOperations.GetStoredEventsCommand(SQLConnection, ContextId, aggregateTypeName, id, startSequenceId);
            if (command == null)
                return Task.FromResult(events);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var eventType = reader.GetString(0);
                var capturedAt = reader.GetDateTime(1);
                var capturedBy = reader.GetString(2);
                var jsonData = reader.GetString(3);
                var storedAt = reader.GetDateTime(4);
                events.Add(new Event.Event(eventType, capturedAt, capturedBy, jsonData, storedAt));
            }
            return Task.FromResult(events);
        }

        public Task CreateTestEnvironment()
        {
            if (ContextId.ContextId == "live")
                throw new ArgumentException("Cannot create a test environment for StorageAdapterContextId='live'");
            return Task.Run(() =>
            {
                string sqlString = SQLOperations.SQLOperations.GetCreateEnvironmentQuery(ContextId);
                SqlCommand command = new SqlCommand(sqlString, SQLConnection);
                command.ExecuteNonQuery();
            });
        }

        public Task DestroyTestEnvironment()
        {
            if (ContextId.ContextId == "live")
                throw new ArgumentException("Cannot destroy a test environment for StorageAdapterContextId='live'");
            return Task.Run(() =>
            {
                string sqlString = SQLOperations.SQLOperations.GetDestroyEnvironmentQuery(ContextId);
                SqlCommand command = new SqlCommand(sqlString, SQLConnection);
                command.ExecuteNonQuery();
            });
        }


        public Task Init()
        {
            return Task.Run(() =>
            {
                SQLConnection.Open();
            });
        }

        public Task<List<Event.Event>?> Store<T>(Aggregate<T> aggregate, bool storeSnapshot) where T : notnull, new()
        {
            var command = SQLOperations.SQLOperations.GetStoreCommand<T>(SQLConnection, ContextId, aggregate, storeSnapshot);
            if (command == null)
                return Task.FromResult(default(List<Event.Event>));
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                if (e.Message.Contains("Violation of PRIMARY KEY constraint"))
                    throw new OCCException<T>();
            }
            return Task.FromResult(aggregate.PendingEvents ?? default(List<Event.Event>));
        }
    }
}