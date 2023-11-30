using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;
using Microsoft.Data.SqlClient;
using Core.StorageAdapters;
using Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations;

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
            throw new NotImplementedException();
        }

        public Task<List<Event.Event>> GetStoredEvents(string aggregateTypeName, string id, long startSequenceId)
        {
            throw new NotImplementedException();
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