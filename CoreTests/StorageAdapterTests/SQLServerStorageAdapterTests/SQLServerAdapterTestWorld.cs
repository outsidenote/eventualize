using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.StorageAdapters;
using Core.StorageAdapters.SQLServerStorageAdapter;

namespace CoreTests.StorageAdapterTests.SQLServerStorageAdapterTests
{
    public class SQLServerAdapterTestWorld
    {
        public StorageAdapterContextId ContextId = new(Guid.NewGuid());
        public SQLServerStorageAdapter StorageAdapter;

        public SQLServerAdapterTestWorld()
        {
            StorageAdapter = new(GetDefaultConnectionString(), ContextId);
        }

        private static SQLServerConnectionData GetDefaultConnectionString()
        {
            return new SQLServerConnectionData();
        }
    }
}