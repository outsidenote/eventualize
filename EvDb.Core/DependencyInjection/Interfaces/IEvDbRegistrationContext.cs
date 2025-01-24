using EvDb.Core;
using EvDb.Core.Internals;

namespace Microsoft.Extensions.DependencyInjection;
public interface IEvDbRegistrationContext: IEvDbRegistrationEntry
{
    EvDbPartitionAddress Address { get;  }
    EvDbStorageContext? Context { get;  }
}