using EvDb.Core;
using EvDb.Core.Internals;

namespace Microsoft.Extensions.DependencyInjection;
public interface IEvDbRegistrationContext : IEvDbRegistrationEntry
{
    EvDbStreamTypeName Address { get; }
    EvDbStorageContext? Context { get; }
}