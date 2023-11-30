using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.SqlServer.Server;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class SQLOperations
    {
        public static string GetCreateEnvironmentQuery(StorageAdapterContextId contextId)
        {
            return CreateEnvironmentQuery.GetSqlString(GetContextIdPrefix(contextId));
        }

        public static string GetDestroyEnvironmentQuery(StorageAdapterContextId contextId)
        {
            return DestroyEnvironmentQuery.GetSqlString(GetContextIdPrefix(contextId));
        }
        public static string GetContextIdPrefix(StorageAdapterContextId contextId)
        {
            if (contextId.ContextId == "live")
                return "";
            return "testcontextid_" + contextId.ContextId.Replace("-", "_") + "_";
        }
        public static SqlCommand? GetStoreCommand<State>(SqlConnection connection, StorageAdapterContextId contextId, Aggregate.Aggregate<State> aggregate, bool isSnapshotStored) where State : notnull, new()
        {
            string? sqlString = StoreQuery.GetStorePendingEventsSqlString(GetContextIdPrefix(contextId), aggregate);
            if (string.IsNullOrEmpty(sqlString))
                return null;
            if (isSnapshotStored)
                sqlString += StoreQuery.GetStoreSnapshotSqlString(GetContextIdPrefix(contextId), aggregate);
            return new SqlCommand(sqlString, connection);
        }

        public static SqlCommand? GetLastStoredSnapshotSequenceIdCommand<State>(SqlConnection connection, StorageAdapterContextId contextId, Aggregate.Aggregate<State> aggregate) where State : notnull, new()
        {
            var sqlString = GetLastStoredSnapshotSequenceIdQuery.GetSqlString(GetContextIdPrefix(contextId), aggregate);
            if (string.IsNullOrEmpty(sqlString))
                return null;
            return new SqlCommand(sqlString, connection);
        }

    }
}