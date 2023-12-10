using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters.SQLServerStorageAdapter.SQLOperations
{
    // TODO: [bnaya 2023-12-10] migration should move to a different project 
    // TODO: [bnaya 2023-12-10] should be internal class, the implementation should be abstract behind IStorageAdapter
    public static class DestroyEnvironmentQuery
    {
        public static string GetSqlString(string contextIdPrefix)
        {
            // TODO: [bnaya 2023-12-10] SQL injection, should use command parameters 
            return $@"
DROP TABLE {contextIdPrefix}event;
DROP TABLE {contextIdPrefix}snapshot;            
            ";
        }

    }
}