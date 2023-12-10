using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core.StorageAdapters;

// TODO: [bnaya 2023-12-10] Should be an interface injected via DI
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