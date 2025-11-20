using Microsoft.Extensions.DependencyInjection;

namespace EvDb.Core.Internals;

/// <summary>
/// The an anchor object for extension method of the storage registration 
/// </summary>
public readonly record struct EvDbRegistrationEntry : IEvDbStreamRegistrationEntry
{
    private readonly IServiceCollection _services;
    private readonly EvDbCloudEventContext? _cloudEventEnvelope;

    public EvDbRegistrationEntry(IServiceCollection services,
                                EvDbCloudEventContext? cloudEventEnvelope)
    {
        _services = services;
        _cloudEventEnvelope = cloudEventEnvelope;
    }

    EvDbCloudEventContext? IEvDbStreamRegistrationEntry.CloudEventContext => _cloudEventEnvelope;

    IServiceCollection IEvDbServiceCollectionWrapper.Services => _services;
}


