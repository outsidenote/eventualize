using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Aggregate;
using Core.Repository;
using Microsoft.Data.SqlClient;
using Core.StorageAdapters;

namespace Core.StorageAdapters.SQLServerStorageAdapter
{
    public class SQLServerStorageAdapter : IStorageAdapter
    {
        private readonly SqlConnection SQLConnection;
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
                InitialCatalog = connectionData.InitialCatalog
            };

            SQLConnection = new SqlConnection(connectionBuilder.ConnectionString);
            ContextId = contextId;
        }
        public Task<long> GetLastStoredSequenceId<T>(Aggregate<T> aggregate) where T : notnull, new()
        {
            throw new NotImplementedException();
        }

        public Task<StoredSnapshotData<T>?> GetLatestSnapshot<T>(string aggregateTypeName, string id) where T : notnull, new()
        {
            throw new NotImplementedException();
        }

        public Task<List<Event.Event>> GetStoredEvents(string aggregateTypeName, string id, long startSequenceId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}