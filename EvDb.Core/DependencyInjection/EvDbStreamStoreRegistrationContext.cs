namespace Microsoft.Extensions.DependencyInjection;

using EvDb.Core;
using EvDb.Core.Internals;

public readonly record struct EvDbStreamStoreRegistrationContext : IEvDbRegistrationContext
{
    private readonly IServiceCollection _services;
    private readonly EvDbStorageContext? _context;
    private readonly EvDbPartitionAddress _address;

    public EvDbStreamStoreRegistrationContext(
        EvDbStorageContext? context,
        EvDbPartitionAddress address,
        IServiceCollection services)
    {
        _services = services;
        _context = context;
        _address = address;
    }

    EvDbStorageContext? IEvDbRegistrationContext.Context => _context;
    EvDbPartitionAddress IEvDbRegistrationContext.Address => _address;
    IServiceCollection IEvDbRegistrationEntry.Services => _services;
}

