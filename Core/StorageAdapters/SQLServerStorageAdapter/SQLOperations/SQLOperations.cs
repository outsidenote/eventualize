using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.SqlServer.Server;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class SQLOperations
    {
        private static string InterpolateFromFile(string filePath, StorageAdapterContextId contextId)
        {
            string fileContent = File.ReadAllText(filePath);
            return string.Format(fileContent, contextId.ContextId);

        }
        public static string GetCreateEnvironmentQuery(StorageAdapterContextId contextId)
        {
            return InterpolateFromFile("./CreateEnvironmentTable.sql", contextId);
        }

        public static string GetDestroyEnvironmentQuery(StorageAdapterContextId contextId)
        {
            return InterpolateFromFile("./DestroyEnvironmentTable.sql", contextId);
        }


    }
}