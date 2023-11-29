using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.StorageAdapters
{
    public class StorageAdapterContextId
    {
        public string ContextId { get; private set; }

        public StorageAdapterContextId()
        {
            ContextId = "live";
        }

        public StorageAdapterContextId(Guid guid)
        {
            ContextId = guid.ToString();
        }
    }
}