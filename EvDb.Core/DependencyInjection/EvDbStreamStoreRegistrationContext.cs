namespace Microsoft.Extensions.DependencyInjection;

using EvDb.Core;
using EvDb.Core.Internals;

public readonly record struct EvDbStreamStoreRegistrationContext : IEvDbRegistrationContext
{
    private readonly IServiceCollection _services;
    private readonly EvDbStorageContext? _context;
    private readonly EvDbStreamTypeName _address;

    public EvDbStreamStoreRegistrationContext(
        EvDbStorageContext? context,
        EvDbStreamTypeName address,
        IServiceCollection services)
    {
        _services = services;
        _context = context;
        _address = address;
    }

    EvDbStorageContext? IEvDbRegistrationContext.Context => _context;
    EvDbStreamTypeName IEvDbRegistrationContext.Address => _address;
    IServiceCollection IEvDbServiceCollectionWrapper.Services => _services;
}

