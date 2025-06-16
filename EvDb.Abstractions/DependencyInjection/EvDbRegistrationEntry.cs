using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Internals;

/// <summary>
/// The an anchor object for extension method of the storage registration 
/// </summary>
public readonly record struct EvDbRegistrationEntry : IEvDbRegistrationEntry
{
    private readonly IServiceCollection _services;

    public EvDbRegistrationEntry(IServiceCollection services)
    {
        _services = services;
    }

    IServiceCollection IEvDbRegistrationEntry.Services => _services;
}


