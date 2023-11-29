using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    public static class DestroyEnvironmentQuery
    {
        public static string GetSqlString(string contextIdPrefix)
        {
            return $@"
DROP TABLE {contextIdPrefix}event;
DROP TABLE {contextIdPrefix}snapshot;            
            ";
        }

    }
}